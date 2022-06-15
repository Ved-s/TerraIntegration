using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using TerraIntegration.DataStructures;
using Terraria;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace TerraIntegration.Utilities
{
    public static class TileMimicking
    {
        public static Dictionary<Point16, TileMimic> MimicData = new();
        public static Dictionary<Point16, TileMimic> TileMimicsInProgress = new();

        public static void Clear()
        {
            MimicData.Clear();
        }

        public static bool IsMimicking(int x, int y) => TileMimicsInProgress.ContainsKey(new(x, y));

        public static ushort GetRealTileType(int x, int y)
        {
            if (TileMimicsInProgress.TryGetValue(new(x, y), out TileMimic mimic))
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
                || !MimicData.TryGetValue(pos, out TileMimic mimic)) return;

            if (!NeedsMimicking(mimic.Type))
            {
                TileObjectData tileObject = TileObjectData.GetTileData(mimic.Type, 0);
                if (tileObject is not null)
                {
                    var orig = tileObject.Origin;

                    MimicData[pos] = new()
                    {
                        Type = mimic.Type,
                        FrameX = (short)(orig.X * 16),
                        FrameY = (short)(orig.Y * 16),
                    };
                }
                return;
            }

            Tile tile = Main.tile[pos.X, pos.Y];

            TileMimic realTile = new();
            realTile.Type = tile.TileType;
            realTile.FrameX = tile.TileFrameX;
            realTile.FrameY = tile.TileFrameY;

            tile.TileType = mimic.Type;
            tile.TileFrameX = mimic.FrameX;
            tile.TileFrameY = mimic.FrameY;

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

            MimicData[pos] = mimicTile;

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

namespace TerraIntegration
{
    public class TileMimic
    {
        public ushort Type;
        public short FrameX;
        public short FrameY;

        public TileMimic() { }

        public TileMimic(ushort type)
        {
            Type = type;
        }

        public TileMimic(ushort type, short frameX, short frameY)
        {
            Type = type;
            FrameX = frameX;
            FrameY = frameY;
        }

        public static void SaveData(TileMimic mimic, BinaryWriter writer)
        {
            if (mimic is null)
            {
                writer.Write(false);
                return;
            }

            writer.Write(true);
            writer.Write(mimic.Type);
            writer.Write(mimic.FrameX);
            writer.Write(mimic.FrameY);
        }

        public static TileMimic LoadData(BinaryReader reader)
        {
            if (!reader.ReadBoolean())
                return null;

            return new(reader.ReadUInt16(), reader.ReadInt16(), reader.ReadInt16());
        }

        public Rectangle FrameRect() => new(FrameX, FrameY, 16, 16);

        public override bool Equals(object obj)
        {
            return obj is TileMimic mimic &&
                   Type == mimic.Type &&
                   FrameX == mimic.FrameX &&
                   FrameY == mimic.FrameY;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Type, FrameX, FrameY);
        }

        public static bool operator ==(TileMimic left, TileMimic right)
        {
            return EqualityComparer<TileMimic>.Default.Equals(left, right);
        }

        public static bool operator !=(TileMimic left, TileMimic right)
        {
            return !(left == right);
        }
    }
}
