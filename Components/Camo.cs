using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerraIntegration.UI;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.ObjectData;
using Terraria.UI;

namespace TerraIntegration.Components
{
    public class CamoData : ComponentData
    {
        public Item CamoTileItem { get; set; }
    }

    public class Camo : Component<CamoData>
    {
        public override string ComponentType => "camo";
        public override string ComponentDisplayName => "Camouflage block";

        public override bool HasRightClickInterface => true;

        public override Vector2 InterfaceOffset => new(24, 0);

        public override void SetStaticDefaults()
        {
            Main.tileSolid[Type] = true;
            Main.tileFrameImportant[Type] = true;
            SetupNewTile();
            TileObjectData.addTile(Type);

            ItemDrop = ModContent.ItemType<Items.Camo>();
        }

        public override UIPanel SetupInterface()
        {
            UIPanel p = new()
            {
                Width = new(58, 0),
                Height = new(58, 0),
                PaddingTop = 0,
                PaddingLeft = 0,
                PaddingRight = 0,
                PaddingBottom = 0,
            };
            UIItemVirtual Slot = new()
            {
                Top = new(8, 0),
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

        public override object SaveCustomDataTag(CamoData data)
        {
            TagCompound tag = new TagCompound();

            if (data.CamoTileItem is not null)
                tag["item"] = ItemIO.Save(data.CamoTileItem);

            return tag;
            
        }
        public override CamoData LoadCustomDataTag(object data)
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
            writer.Write(data.CamoTileItem is not null);
            if (data.CamoTileItem is not null)
                ItemIO.Send(data.CamoTileItem, writer, true);
        }
        public override CamoData ReceiveCustomData(BinaryReader reader)
        {
            CamoData cd = new();
            if (reader.ReadBoolean())
                cd.CamoTileItem = ItemIO.Receive(reader, true);

            return cd;
        }

        public void CamoChanged(Item camo, Point16 pos, bool noSync)
        {
            if (camo is not null && camo.createTile > -1)
            {
                TileMimicking.MimicType[pos] = (ushort)camo.createTile;
            }
            else TileMimicking.MimicType.Remove(pos);

            WorldGen.SquareTileFrame(pos.X, pos.Y, false);

            if (!noSync && Main.netMode != NetmodeID.SinglePlayer)
            {
                CamoData data = GetData(pos);

                if (data.CamoTileItem is null) CreatePacket(pos, 0).Send();
                else
                {
                    ModPacket p = CreatePacket(pos, 1);
                    ItemIO.Send(data.CamoTileItem, p, true);
                    p.Send();
                }
            }
        }

        public override bool HandlePacket(Point16 pos, ushort messageType, BinaryReader reader, int whoAmI, ref bool broadcast)
        {
            if (messageType == 0) 
            {
                GetData(pos).CamoTileItem = null;
                CamoChanged(null, pos, true);
                broadcast = true;
                return true;
            }
            if (messageType == 1)
            {
                Item item = ItemIO.Receive(reader);

                GetData(pos).CamoTileItem = item;
                CamoChanged(item, pos, true);
                broadcast = true;
                return true;
            }
            return false;
        }

        public override bool PreDraw(int i, int j, SpriteBatch spriteBatch)
        {
            CamoData data = GetData(new(i, j));
            if (data.CamoTileItem is null
                || data.CamoTileItem.createTile <= -1
                || !TileMimicking.MimicResult.TryGetValue(new(i, j), out TileMimicking.TileMimic mimic)
                || mimic.Type != data.CamoTileItem.createTile
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
