﻿using CustomTreeLib;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

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
        public override int MaxHeight => 12;
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
            TreeTileInfo info = TreeTileInfo.GetInfo(x, y);

            if (info.IsLeafy)
                Item.NewItem(WorldGen.GetItemSource_FromTileBreak(x, y), new Vector2(x, y) * 16, Acorn.Type);

            if (Main.rand.NextBool(2))
                Item.NewItem(WorldGen.GetItemSource_FromTileBreak(x, y), new Vector2(x, y) * 16, ModContent.ItemType<Items.Bluewood>(), Main.rand.Next(1,3));

            return false;
        }

        public override void TileFrame(int x, int y)
        {
            TreeTileInfo info = TreeTileInfo.GetInfo(x, y);

            if (Main.tile[x, y + 1].TileType == Tile.Type || info.WithRoots) return;
            base.TileFrame(x, y);
        }

        public override void RandomUpdate(int x, int y)
        {
            CheckCutSides(x, y, out bool top, out bool left, out bool right);

            Tile t = Main.tile[x, y];

            if (top) PlaceSap(x, y - 1, 0, t.TileColor);
            if (left) PlaceSap(x - 1, y, 1, t.TileColor);
            if (right) PlaceSap(x + 1, y, 2, t.TileColor);
        }

        public void PlaceSap(int x, int y, int side, byte color) 
        {
            Tile t = Main.tile[x, y];

            if (t.HasTile)
                return;

            t.HasTile = true;
            t.TileType = (ushort)ModContent.TileType<Tiles.CrystallizedSap>();
            t.TileFrameY = (short)(side * 18);
            t.TileFrameX = (short)(Main.rand.Next(2) * 18);
            t.TileColor = color;

            WorldGen.SquareTileFrame(x, y);
        }

        void CheckCutSides(int x, int y, out bool topCut, out bool leftCut, out bool rightCut) 
        {
            topCut = false;
            leftCut = false;
            rightCut = false;

            Tile tile = Main.tile[x, y];

            if (!TileID.Sets.IsATreeTrunk[tile.TileType]) return;

            TreeTileInfo info = TreeTileInfo.GetInfo(tile);

            bool checkTopCut = !info.IsTop
                && info.Type != TreeTileType.Root
                && info.Type != TreeTileType.Branch
                && info.Type != TreeTileType.LeafyBranch;
            bool checkLeftCut = false;
            bool checkRightCut = false;

            if (info.WithBranches || info.WithRoots) 
            {
                checkLeftCut = info.Side == TreeTileSide.Left || info.Side == TreeTileSide.Center;
                checkRightCut = info.Side == TreeTileSide.Right || info.Side == TreeTileSide.Center;
            }

            Tile left = Main.tile[x - 1, y];
            Tile top = Main.tile[x, y - 1];
            Tile right = Main.tile[x + 1, y];

            if (checkTopCut && top.TileType != tile.TileType)
                topCut = true;

            if (checkLeftCut && left.TileType != tile.TileType)
                leftCut = true;

            if (checkRightCut && right.TileType != tile.TileType)
                rightCut = true;
        }
    }
}