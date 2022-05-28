using Microsoft.Xna.Framework;

namespace TerraIntegration.DataStructures
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
