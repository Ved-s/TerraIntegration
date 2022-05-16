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
}
