using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TerraIntegration.DataStructures
{
    public struct Rectf
    {
        public float X, Y, Width, Height;

        public float Left => X;
        public float Right => X + Width;
        public float Top => Y;
        public float Bottom => Y + Height;

        public Vector2 Center => new(X + Width / 2, Y + Height / 2);

        public Vector2 Position
        {
            get => new(X, Y);
            set { X = value.X; Y = value.Y; }
        }

        public Vector2 Size
        {
            get => new(Width, Height);
            set { Width = value.X; Height = value.Y; }
        }

        public Rectf(float x, float y, float width, float height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        public Rectf(Vector2 position, Vector2 size)
        {
            (X, Y) = position;
            (Width, Height) = size;
        }

        public bool Intersects(Rectf rect)
        {
            return rect.Left < Right && Left < rect.Right && rect.Top < Bottom && Top < rect.Bottom;
        }
    }
}
