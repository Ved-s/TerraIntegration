using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TerraIntegration
{
    public struct Point16
    {
        public short X, Y;

        public Point16(short x, short y)
        {
            X = x;
            Y = y;
        }

        public Point16(int x, int y)
        {
            X = (short)x;
            Y = (short)y;
        }

        public static Point16 operator +(Point16 a, Point16 b) => new(a.X + b.X, a.Y + b.Y);
        public static Point16 operator -(Point16 a, Point16 b) => new(a.X - b.X, a.Y - b.Y);

        public static bool operator ==(Point16 a, Point16 b) => a.X == b.X && a.Y == b.Y;
        public static bool operator !=(Point16 a, Point16 b) => a.X != b.X || a.Y != b.Y;

        public static explicit operator Point16(Vector2 v) => new((int)v.X, (int)v.Y);
        public static implicit operator Point(Point16 p) => new(p.X, p.Y);

        public Vector2 ToVector2() => new Vector2(X, Y);
    }

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
