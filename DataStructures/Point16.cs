using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TerraIntegration.DataStructures
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

        public override string ToString()
        {
            return $"({X}, {Y})";
        }
    }
}
