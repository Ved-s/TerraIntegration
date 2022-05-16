using CustomTreeLib;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;

namespace TerraIntegration
{
    public class Bluewood : CustomTree
    {
        public override string SaplingTexture => "TerraIntegration/Assets/Trees/Bluewood_Sapling";
        public override string AcornTexture => "TerraIntegration/Assets/Trees/Bluewood_Seed";

        public override string TileTexture => "TerraIntegration/Assets/Trees/Bluewood";
        public override string TopTexture => "TerraIntegration/Assets/Trees/Bluewood_Top";
        public override string BranchTexture => "TerraIntegration/Assets/Trees/Bluewood_Branches";

        public override string LeafTexture => "TerraIntegration/Assets/Trees/Bluewood_Leaf";

        public override int[] ValidGroundTiles => new int[] { TileID.Grass };

        public override int GrowChance => 2;
        public override int SaplingStyles => 3;
        public override int MaxHeight => 8;
        public override int MinHeight => 2;

        public override string DefaultAcornName => "Bluewood Seed";

        public override Color? MapColor => new(0x3e, 0x5f, 0x86);
        public override string MapName => "Bluewood Tree";

        public override Color? SaplingMapColor => new(0x3e, 0x5f, 0x86);
        public override string SaplingMapName => "Bluewood Sapling";

        public override bool GetTreeFoliageData(int i, int j, int xoffset, ref int treeFrame, out int floorY, out int topTextureFrameWidth, out int topTextureFrameHeight)
        {
            topTextureFrameWidth = 80;
            topTextureFrameHeight = 80;
            floorY = 0;
            return true;
        }

        public override bool Drop(int x, int y)
        {
            if (IsBranchTile(x, y))
            {
                if (Main.rand.NextBool(2))
                    Item.NewItem(WorldGen.GetItemSource_FromTileBreak(x, y), new Vector2(x, y) * 16, Acorn.Type);
            }

            return false;
        }
    }
}
