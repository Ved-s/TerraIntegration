using CustomTreeLib;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerraIntegration.DataStructures;
using TerraIntegration.UI;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.ObjectData;

namespace TerraIntegration.Components
{
    public class SapCollectorData : ComponentData
    {
        public Items.Materials.CrystallizedSap Item;
    }

    public class SapCollector : Component<SapCollectorData>
    {
        public override string Texture => "TerraIntegration/Assets/Tiles/SapCollector";

        public override string ComponentType => "sap";
        public override string ComponentDisplayName => "Sap collector";

        public override bool HasCustomInterface => true;
        public override ushort DefaultUpdateFrequency => 30;
        public override bool ConfigurableFrequency => false;

        public override Vector2 InterfaceOffset => new Vector2(24, 0);

        public const int SlotCap = 32;

        public UIText EfficiencyText;

        public override void OnUpdate(Point16 pos)
        {
            base.OnUpdate(pos);

            if (InterfacePos == pos)
                UpdateEfficiencyText();
        }

        public override void RandomUpdate(int i, int j)
        {
            if (Main.rand.NextFloat() <= GetEfficiency(new(i, j)) / 5)
            {
                SapCollectorData data = GetData(new(i, j));
                if (data.Item is null || data.Item.Item.stack < SlotCap)
                {
                    if (data.Item is null)
                        data.Item = Util.CreateModItem<Items.Materials.CrystallizedSap>();
                    
                    else data.Item.Item.stack++;
                }
            }
        }

        public override void SetStaticDefaults()
        {
            Main.tileSolid[Type] = false;
            Main.tileFrameImportant[Type] = true;

            SetupNewTile();
            TileObjectData.addTile(Type);

            ItemDrop = ModContent.ItemType<Items.ComponentItems.SapCollector>();
        }

        public override UIPanel SetupInterface()
        {
            UIPanel p = new()
            {
                MinWidth = new(160, 0),

                Width = new(0, 1),
                Height = new(58, 0),
                PaddingTop = 0,
                PaddingLeft = 0,
                PaddingRight = 0,
                PaddingBottom = 0,
            };
            UIItemVirtual Slot = new()
            {
                Top = new(8, 0),
                Left = new(-50, 1f),

                MaxSlotCapacity = SlotCap,

                ItemValidator = (item) => item.type == ModContent.ItemType<Items.Materials.CrystallizedSap>(),

                GetItem = () =>
                ModContent.GetInstance<ComponentInterface>()
                .InterfaceComponent.GetDataOrNull<SapCollectorData>()?
                .Item?.Item,

                SetItem = (item) =>
                {
                    PositionedComponent component = ModContent.GetInstance<ComponentInterface>().InterfaceComponent;
                    SapCollectorData data = component.GetDataOrNull<SapCollectorData>();
                    if (data is not null)
                    {
                        data.Item = (item?.ModItem as Items.Materials.CrystallizedSap);
                        NotifyItemChange(data.Item, component.Pos);
                    }
                }

            };
            EfficiencyText = new($"Efficiency:\n")
            {
                Top = new(6, 0),
                Left = new(6, 0)
            };
            p.Append(EfficiencyText);
            p.Append(Slot);

            return p;
        }

        public override void UpdateInterface(Point16 pos)
        {
            UpdateEfficiencyText();
        }

        public override bool CanPlace(int i, int j)
        {
            return IsBluewoodAround(i, j);
        }

        public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak)
        {
            if (!noBreak && !IsBluewoodAround(i,j))
                WorldGen.KillTile(i, j);

            return false;
        }

        public bool IsBluewoodAround(int x, int y) 
        {
            ushort bluewood = ModContent.GetInstance<Bluewood>().Tile.Type;

            if (Main.tile[x-1, y].TileType == bluewood && TreeTileInfo.GetInfo(x-1, y).IsCenter)
                return true;

            if (Main.tile[x+1, y].TileType == bluewood && TreeTileInfo.GetInfo(x+1, y).IsCenter)
                return true;

            return false;
        }

        public void UpdateEfficiencyText()
        {
            if (EfficiencyText is null || !InterfaceVisible) return;

            EfficiencyText.SetText($"Efficiency:\n{GetEfficiency(InterfacePos) * 100:0.0}%");
        }

        public float GetEfficiency(Point16 pos) 
        {
            if (!IsBluewoodAround(pos.X, pos.Y)) 
                return 0;

            bool left = Framing.GetTileSafely(pos.X - 1, pos.Y).TileType == ModContent.GetInstance<Bluewood>().Tile.Type;

            Point16 tilepos = left ? new(pos.X - 1, pos.Y) : new(pos.X + 1, pos.Y);
            TreeTileInfo info = TreeTileInfo.GetInfo(tilepos.X, tilepos.Y);

            if (!info.WithBranches && !info.WithRoots || info.Side == (left? TreeTileSide.Left : TreeTileSide.Right)) 
                return 0;

            float efficiency = 0f;

            if (info.WithBranches) efficiency += .3f;
            else efficiency += .15f;

            TreeStats stats = TreeGrowing.GetTreeStats(tilepos.X, tilepos.Y);

            if (stats.LeftRoot) efficiency += .05f;
            if (stats.RightRoot) efficiency += .05f;

            efficiency += stats.TotalBlocks * .01f;
            if (stats.HasTop && !stats.BrokenTop)
                efficiency += .25f;

            efficiency += stats.LeafyBranches * .05f;

            return efficiency;
        }

        private void NotifyItemChange(Items.Materials.CrystallizedSap item, Point16 pos) 
        {
            if (Main.netMode == NetmodeID.SinglePlayer) return;

            ModPacket p = CreatePacket(pos, 0);
            p.Write(item is null ? 0 : item.Item.stack);
            p.Send();
        }

        public override bool HandlePacket(Point16 pos, ushort messageType, BinaryReader reader, int whoAmI, ref bool broadcast)
        {
            if (messageType == 0)
            {
                SapCollectorData data = GetData(pos);

                int stack = reader.ReadInt32();
                if (stack == 0) data.Item = null;
                else 
                {
                    if (data.Item is null) 
                        data.Item = Util.CreateModItem<Items.Materials.CrystallizedSap>(stack);
                    else data.Item.Item.stack = stack;
                }

                broadcast = true;
                return true;
            }

            return false;
        }

        public override void SendCustomData(SapCollectorData data, BinaryWriter writer)
        {
            writer.Write(data.Item is null ? 0 : data.Item.Item.stack);
        }
        public override SapCollectorData ReceiveCustomData(BinaryReader reader, Point16 pos)
        {
            SapCollectorData data = new();

            int stack = reader.ReadInt32();
            if (stack == 0) data.Item = null;
            else
            {
                if (data.Item is null)
                    data.Item = Util.CreateModItem<Items.Materials.CrystallizedSap>(stack);
                else data.Item.Item.stack = stack;
            }

            return data;
        }

        public override object SaveCustomDataTag(SapCollectorData data)
        {
            TagCompound tag = new();

            if (data.Item is not null)
                tag["stack"] = data.Item.Item.stack;

            return tag;
        }
        public override SapCollectorData LoadCustomDataTag(object data, Point16 pos)
        {
            SapCollectorData sdata = new();

            if (data is TagCompound tag) 
            {
                if (tag.ContainsKey("stack"))
                {
                    int stack = tag.GetInt("stack");
                    if (stack > 0)
                    {
                        if (sdata.Item is null)
                            sdata.Item = Util.CreateModItem<Items.Materials.CrystallizedSap>(stack);
                        else sdata.Item.Item.stack = stack;
                    }
                }
            }

            return sdata;
        }
    }
}
