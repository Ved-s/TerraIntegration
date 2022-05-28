using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.ObjectData;

namespace TerraIntegration
{
    public static class TileMimicking
    {
        public static Dictionary<Point16, ushort> MimicType = new();
        public static Dictionary<Point16, TileMimic> MimicResult = new();

        public static Dictionary<Point16, TileMimic> TileMimicsInProgress = new();

        public class TileMimic
        {
            public ushort Type;
            public short FrameX;
            public short FrameY;

            public Rectangle FrameRect() => new(FrameX, FrameY, 16, 16);
        }

        public static void Clear()
        {
            MimicType.Clear();
            MimicResult.Clear();
        }

        public static ushort GetRealTileType(int x, int y)
        {
            if (TileMimicsInProgress.TryGetValue(new(x,y), out TileMimic mimic))
                return mimic.Type;
            return Main.tile[x, y].TileType;
        }

        public static void BeforeTileFrame(int x, int y)
        {
            PrepareTileMimicking(new(x, y));
            PrepareTileMimicking(new(x, y - 1));
            PrepareTileMimicking(new(x, y + 1));
            PrepareTileMimicking(new(x - 1, y));
            PrepareTileMimicking(new(x + 1, y));
            PrepareTileMimicking(new(x - 1, y - 1));
            PrepareTileMimicking(new(x - 1, y + 1));
            PrepareTileMimicking(new(x + 1, y - 1));
            PrepareTileMimicking(new(x + 1, y + 1));
        }

        public static void AfterTileFrame(int x, int y)
        {
            FinishTileMimicking(new(x, y));
            FinishTileMimicking(new(x, y - 1));
            FinishTileMimicking(new(x, y + 1));
            FinishTileMimicking(new(x - 1, y));
            FinishTileMimicking(new(x + 1, y));
            FinishTileMimicking(new(x - 1, y - 1));
            FinishTileMimicking(new(x - 1, y + 1));
            FinishTileMimicking(new(x + 1, y - 1));
            FinishTileMimicking(new(x + 1, y + 1));
        }

        public static void PrepareTileMimicking(Point16 pos)
        {
            if (TileMimicsInProgress.ContainsKey(pos) 
                || !MimicType.TryGetValue(pos, out ushort mimicType)) return;

            if (!NeedsMimicking(mimicType))
            {
                TileObjectData tileObject = TileObjectData.GetTileData(mimicType, 0);
                if (tileObject is not null)
                {
                    var orig = tileObject.Origin;

                    MimicResult[pos] = new()
                    {
                        Type = mimicType,
                        FrameX = (short)(orig.X * 16),
                        FrameY = (short)(orig.Y * 16),
                    };
                }
                return;
            }

            TileMimic mimicTile;
            if (MimicResult.TryGetValue(pos, out mimicTile))
            {
                if (mimicTile.Type != mimicType)
                    mimicTile = new();
            } else mimicTile = new();
            Tile tile = Main.tile[pos.X, pos.Y];
            mimicTile.Type = mimicType;

            TileMimic realTile = new();
            realTile.Type = tile.TileType;
            realTile.FrameX = tile.TileFrameX;
            realTile.FrameY = tile.TileFrameY;

            tile.TileType = mimicTile.Type;
            tile.TileFrameX = mimicTile.FrameX;
            tile.TileFrameY = mimicTile.FrameY;

            TileMimicsInProgress.Add(pos, realTile);
        }

        public static void FinishTileMimicking(Point16 pos)
        {
            if (!TileMimicsInProgress.TryGetValue(pos, out TileMimic realTile)) return;

            Tile tile = Main.tile[pos.X, pos.Y];

            TileMimic mimicTile = new();
            mimicTile.Type = tile.TileType;
            mimicTile.FrameX = tile.TileFrameX;
            mimicTile.FrameY = tile.TileFrameY;

            MimicResult[pos] = mimicTile;

            tile.TileType = realTile.Type;
            tile.TileFrameX = realTile.FrameX;
            tile.TileFrameY = realTile.FrameY;

            TileMimicsInProgress.Remove(pos);
        }

        public static bool PreventKillTile(int x, int y)
        {
            return TileMimicsInProgress.ContainsKey(new(x, y));
        }

        public static bool NeedsMimicking(int type)
        {
            return !Main.tileFrameImportant[type];
        }
    }
}
