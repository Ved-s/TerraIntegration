using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System.Collections.Generic;
using System.IO;
using TerraIntegration.Basic;
using TerraIntegration.DataStructures;
using TerraIntegration.UI;
using TerraIntegration.Utilities;
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

        internal TileMimic LastMimic = null;
    }

    public class Camo : Component<CamoData>
    {
        public override string TypeName => "camo";
        public override string TypeDefaultDisplayName => "Camouflage block";
        public override string TypeDefaultDescription => "Camouflage block can be decorated\ninto another blocks";

        public override bool HasCustomInterface => true;

        public override ushort DefaultUpdateFrequency => 60;
        public override bool ConfigurableFrequency => true;

        public override bool CanHaveVariables => true;

        public Camo() 
        {
            VariableInfo = new ComponentVariableInfo[]
            {
                new()
                {
                    AcceptVariableReturnTypes = new[] { typeof(Values.Objects.Tile) },
                    VariableName = "Tile",
                    VariableSlot = "camoTile"
                }
            };
        }

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
                    return item.createTile >= TileID.Dirt;
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
            TileMimicking.MimicData.Remove(pos);
        }
        public override void OnLoaded(Point16 pos)
        {
            base.OnLoaded(pos);
            CamoData data = GetData(pos);
            if (data.CamoTileItem is not null && data.CamoTileItem.createTile > -1)
            {
                TileMimicking.MimicData[pos] = new((ushort)data.CamoTileItem.createTile);
            }
        }
        public override void OnUpdate(Point16 pos)
        {
            base.OnUpdate(pos);

            if (Networking.Client) return;

            CamoData data = GetData(pos);

            if (data.TryGetVariable("camoTile", out Variable var))
            {
                data.LastErrors.Clear();
                Values.Objects.Tile tile = var.GetValue(data.System, data.LastErrors) as Values.Objects.Tile;
                var.SetLastValue(tile, data.System);

                TileMimic mimic = null;

                if (tile is not null && data.LastErrors.Count <= 0 && tile.TileType >= 0)
                {
                    mimic = new();
                    mimic.Type = (ushort)tile.TileType;
                    mimic.FrameX = (short)tile.TileFrameX;
                    mimic.FrameY = (short)tile.TileFrameY;
                }
                data.SyncErrors();
                CamoChanged(mimic, pos, false);
            }
        }

        public override TagCompound SaveCustomDataTag(CamoData data)
        {
            TagCompound tag = new TagCompound();

            if (data.CamoTileItem is not null)
                tag["item"] = ItemIO.Save(data.CamoTileItem);

            return tag;

        }
        public override CamoData LoadCustomDataTag(TagCompound data, Point16 pos)
        {
            CamoData cd = new();

            if (data is null)
                return cd;

            if (data.ContainsKey("item"))
                cd.CamoTileItem = ItemIO.Load(data.GetCompound("item"));

            return cd;
        }
        public override void SendCustomData(CamoData data, BinaryWriter writer)
        {
            bool item = data.CamoTileItem is not null;

            writer.Write(item);
            if (item)
            {
                ItemIO.Send(data.CamoTileItem, writer, true);
            }
            TileMimic.SaveData(data.LastMimic, writer);
        }
        public override CamoData ReceiveCustomData(BinaryReader reader, Point16 pos)
        {
            CamoData cd = new();
            if (reader.ReadBoolean())
            {
                cd.CamoTileItem = ItemIO.Receive(reader, true);
                CamoChanged(cd.CamoTileItem, pos, true);  
            }
            CamoChanged(TileMimic.LoadData(reader), pos, true);
            return cd;
        }

        public void CamoChanged(Item camo, Point16 pos, bool noSync)
        {
            CamoData data = GetData(pos);

            TileMimic mimic = null;

            if (camo is not null && camo.createTile >= TileID.Dirt)
            {
                mimic = new((ushort)camo.createTile);
            }
            CamoChanged(mimic, pos, true);

            if (!noSync && Main.netMode != NetmodeID.SinglePlayer)
            {
                if (data.CamoTileItem is null) CreatePacket(pos, 0).Send();
                else
                {
                    ModPacket p = CreatePacket(pos, 1);
                    ItemIO.Send(data.CamoTileItem, p);
                    p.Send();
                }
            }
        }
        public void CamoChanged(TileMimic mimic, Point16 pos, bool noSync)
        {
            CamoData data = GetData(pos);

            if (Util.ObjectsNullEqual(mimic, data.LastMimic)) return;
            data.LastMimic = mimic;
            if (mimic is not null)
            {
                TileMimicking.MimicData[pos] = mimic;
            }
            else TileMimicking.MimicData.Remove(pos);

            WorldGen.SquareTileFrame(pos.X, pos.Y, false);

            if (!noSync && Main.netMode != NetmodeID.SinglePlayer)
            {
                ModPacket p = CreatePacket(pos, 2);
                TileMimic.SaveData(mimic, p);
                p.Send();
            }
        }

        public override bool HandlePacket(Point16 pos, ushort messageType, BinaryReader reader, int whoAmI, ref bool broadcast)
        {
            switch (messageType)
            {
                case 0:
                    GetData(pos).CamoTileItem = null;
                    CamoChanged(null as Item, pos, true);
                    broadcast = true;
                    return true;
                case 1:
                    Item item = ItemIO.Receive(reader);
                    GetData(pos).CamoTileItem = item;
                    CamoChanged(item, pos, true);
                    broadcast = true;
                    return true;
                case 2:
                    CamoChanged(TileMimic.LoadData(reader), pos, true);
                    broadcast = true;
                    return true;

                default:
                    return false;
            }
        }

        public override bool PreDraw(int i, int j, SpriteBatch spriteBatch)
        {
            CamoData data = GetData(new(i, j));
            bool anyCamo = data.CamoTileItem is not null || data.HasVariable("camoTile"); 

            if (!anyCamo
                || data.LastMimic is null
                || !TileMimicking.MimicData.TryGetValue(new(i, j), out TileMimic mimic)
                || data.LastMimic.Type != mimic.Type
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
