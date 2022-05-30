using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System.Collections.Generic;
using System.IO;
using TerraIntegration.UI;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.ObjectData;

namespace TerraIntegration.Components
{
    public class CamoData : ComponentData
    {
        public Item CamoTileItem { get; set; }

        internal int LastTileType = -1;

        public override void Loaded()
        {
            if (HasVariable("camoTile"))
                CamoTileItem = GetVariableItem("camoTile").Item;
        }
    }

    public class Camo : Component<CamoData>
    {
        public override string ComponentType => "camo";
        public override string ComponentDisplayName => "Camouflage block";

        public override bool HasCustomInterface => true;

        public override ushort DefaultUpdateFrequency => 60;
        public override bool ConfigurableFrequency => true;

        public override bool CanHaveVariables => true;

        public override Vector2 InterfaceOffset => new(24, 0);

        List<Error> Errors = new();

        public override void SetStaticDefaults()
        {
            Main.tileSolid[Type] = true;
            Main.tileFrameImportant[Type] = true;
            SetupNewTile();
            TileObjectData.addTile(Type);

            ItemDrop = ModContent.ItemType<Items.ComponentItems.Camo>();
        }

        public override UIPanel SetupInterface()
        {
            UIPanel p = new()
            {
                Width = new(0, 1),
                MinWidth = new(58, 0),
                Height = new(58, 0),
                PaddingTop = 0,
                PaddingLeft = 0,
                PaddingRight = 0,
                PaddingBottom = 0,
            };
            UIItemVirtual Slot = new()
            {
                Top = new(-21, 0.5f),
                Left = new(-21, 0.5f),

                MaxSlotCapacity = 1,

                GetItem = () =>
                ModContent.GetInstance<ComponentInterface>()
                .InterfaceComponent.GetDataOrNull<CamoData>()?
                .CamoTileItem,

                SetItem = (item) =>
                {
                    PositionedComponent component = ModContent.GetInstance<ComponentInterface>().InterfaceComponent;
                    CamoData data = component.GetDataOrNull<CamoData>();
                    if (data is not null)
                    {
                        data.CamoTileItem = item;
                        CamoChanged(item, component.Pos, false);
                    }
                },

                ItemValidator = (item) =>
                {
                    return item.createTile >= TileID.Dirt
                    || item.ModItem is Items.Variable var
                    && var.Var.VariableReturnType == typeof(Values.Tile);
                }
            };
            p.Append(Slot);

            return p;
        }

        public override void OnKilled(Point16 pos)
        {
            CamoData data = GetData(pos);
            if (data.CamoTileItem is not null)
            {
                Util.DropItemInWorld(data.CamoTileItem, pos.X * 16, pos.Y * 16);
                data.CamoTileItem = null;
            }

            base.OnKilled(pos);
            TileMimicking.MimicType.Remove(pos);
        }
        public override void OnLoaded(Point16 pos)
        {
            base.OnLoaded(pos);
            CamoData data = GetData(pos);
            if (data.CamoTileItem is not null && data.CamoTileItem.createTile > -1)
            {
                TileMimicking.MimicType[pos] = (ushort)data.CamoTileItem.createTile;
            }
        }
        public override void OnUpdate(Point16 pos)
        {
            base.OnUpdate(pos);

            if (Networking.Client) return;

            CamoData data = GetData(pos);

            if (data.CamoTileItem?.ModItem is Items.Variable var && var.Var.VariableReturnType == typeof(Values.Tile))
            {
                Errors.Clear();
                Values.Tile tile = var.Var.GetValue(data.System, Errors) as Values.Tile;
                if (tile is null || Errors.Count > 0)
                {
                    CamoChanged(-1, pos, false);
                }
                else CamoChanged(tile.TileType, pos, false);
            }
        }

        public override object SaveCustomDataTag(CamoData data)
        {
            TagCompound tag = new TagCompound();

            if (data.CamoTileItem is not null && !data.HasVariable("camoTile"))
                tag["item"] = ItemIO.Save(data.CamoTileItem);

            return tag;

        }
        public override CamoData LoadCustomDataTag(object data, Point16 pos)
        {
            CamoData cd = new();

            if (data is not TagCompound tag)
                return cd;

            if (tag.ContainsKey("item"))
                cd.CamoTileItem = ItemIO.Load(tag.GetCompound("item"));

            return cd;
        }
        public override void SendCustomData(CamoData data, BinaryWriter writer)
        {
            bool item = data.CamoTileItem is not null && !data.HasVariable("camoTile");

            writer.Write(item);
            if (item)
            {
                ItemIO.Send(data.CamoTileItem, writer, true);
                if (data.LastTileType >= 0 && data.CamoTileItem.createTile < 0)
                    writer.Write(data.LastTileType);
                else 
                    writer.Write(-1);
            }
        }
        public override CamoData ReceiveCustomData(BinaryReader reader, Point16 pos)
        {
            CamoData cd = new();
            if (reader.ReadBoolean())
            {
                cd.CamoTileItem = ItemIO.Receive(reader, true);
                CamoChanged(cd.CamoTileItem, pos, true);
                int tileType = reader.ReadInt32();
                if (tileType >= 0)
                    CamoChanged(tileType, pos, true);
            }

            return cd;
        }

        public void CamoChanged(Item camo, Point16 pos, bool noSync)
        {
            CamoData data = GetData(pos);

            if (camo?.ModItem is Items.Variable var)
            {
                data.SetVariable("camoTile", var);
                OnVariableChanged(pos, "camoTile");
                noSync = true;
            }

            if (camo is null || camo.createTile < TileID.Dirt)
            {
                if (camo is null && data.HasVariable("camoTile"))
                {
                    data.ClearVariable("camoTile");
                    OnVariableChanged(pos, "camoTile");
                }
                CamoChanged(-1, pos, true);
                return;
            }
            CamoChanged(camo.createTile, pos, true);

            if (!noSync && Main.netMode != NetmodeID.SinglePlayer)
            {
                if (data.CamoTileItem is null) CreatePacket(pos, 0).Send();
                else
                {
                    ModPacket p = CreatePacket(pos, 1);
                    ItemIO.Send(data.CamoTileItem, p, true);
                    p.Send();
                }
            }
        }
        public void CamoChanged(int tileType, Point16 pos, bool noSync)
        {
            CamoData data = GetData(pos);

            if (tileType == data.LastTileType) return;
            data.LastTileType = tileType;
            if (tileType > -1)
            {
                TileMimicking.MimicType[pos] = (ushort)tileType;
            }
            else TileMimicking.MimicType.Remove(pos);

            WorldGen.SquareTileFrame(pos.X, pos.Y, false);

            if (!noSync && Main.netMode != NetmodeID.SinglePlayer)
            {
                ModPacket p = CreatePacket(pos, 2);
                p.Write(tileType);
                p.Send();
            }
        }

        public override bool HandlePacket(Point16 pos, ushort messageType, BinaryReader reader, int whoAmI, ref bool broadcast)
        {
            switch (messageType)
            {
                case 0:
                    GetData(pos).CamoTileItem = null;
                    CamoChanged(null, pos, true);
                    broadcast = true;
                    return true;
                case 1:
                    Item item = ItemIO.Receive(reader);
                    GetData(pos).CamoTileItem = item;
                    CamoChanged(item, pos, true);
                    broadcast = true;
                    return true;
                case 2:
                    int tileType = reader.ReadInt32();
                    CamoChanged(tileType, pos, true);
                    broadcast = true;
                    return true;

                default:
                    return false;
            }
        }

        public override bool PreDraw(int i, int j, SpriteBatch spriteBatch)
        {
            CamoData data = GetData(new(i, j));
            int type = data.LastTileType;

            if (data.CamoTileItem is not null && data.CamoTileItem.createTile >= TileID.Dirt)
                type = data.CamoTileItem.createTile;

            if (type <= -1
                || !TileMimicking.MimicResult.TryGetValue(new(i, j), out TileMimicking.TileMimic mimic)
                || mimic.Type != type
                || Main.LocalPlayer.CanSeeInvisibleBlocks) return true;

            Asset<Texture2D> tile = TextureAssets.Tile[mimic.Type];
            if (!tile.IsLoaded)
                tile = Main.Assets.Request<Texture2D>(tile.Name, AssetRequestMode.ImmediateLoad);

            Vector2 screen = (new Vector2(i + 12, j + 12) * 16) - Main.screenPosition;
            Main.spriteBatch.Draw(tile.Value, screen, mimic.FrameRect(), Lighting.GetColor(new(i, j)));

            return false;
        }
    }
}
