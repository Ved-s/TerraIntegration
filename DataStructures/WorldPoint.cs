using Microsoft.Xna.Framework;
using System;

namespace TerraIntegration.DataStructures
{
    public struct WorldPoint
    {
        public short X, Y;
        public bool Wall;

        public WorldPoint(short x, short y, bool wall)
        {
            X = x;
            Y = y;
            Wall = wall;
        }

        public WorldPoint(Point16 pos, bool wall)
        {
            X = pos.X;
            Y = pos.Y;
            Wall = wall;
        }

        public WorldPoint WithOffset(short x, short y)
        {
            return new WorldPoint((short)(X + x), (short)(Y + y), Wall);
        }

        public WorldPoint WithWall(bool wall)
        {
            return new WorldPoint(X, Y, wall);
        }

        public Point ToPoint()
        {
            return new Point(X, Y);
        }

        public Point16 ToPoint16()
        {
            return new Point16(X, Y);
        }

        public override bool Equals(object obj)
        {
            return obj is WorldPoint point &&
                   X == point.X &&
                   Y == point.Y &&
                   Wall == point.Wall;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(X, Y, Wall);
        }
    }
}
