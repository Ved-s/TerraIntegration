using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TerraIntegration
{
    public class SpriteSheet
    {
        public string Texture;
        public Point SpriteSize;

        public SpriteSheet(string texture, Point spriteSize)
        {
            Texture = texture;
            SpriteSize = spriteSize;
        }
    }

    public struct SpriteSheetPos 
    {
        public SpriteSheet SpriteSheet;
        public int X;
        public int Y;

        public SpriteSheetPos(SpriteSheet spriteSheet, int spriteX, int spriteY)
        {
            SpriteSheet = spriteSheet;
            X = spriteX;
            Y = spriteY;
        }

        public SpriteSheetPos(int spriteX, int spriteY)
        {
            SpriteSheet = null;
            X = spriteX;
            Y = spriteY;
        }
    }
}
