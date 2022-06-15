using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.GameContent;

namespace TerraIntegration.Utilities
{
    public static class Drawing
    {
        public static void DrawLine(this SpriteBatch spriteBatch, Vector2 pos, float angle, float length, Color color, float thickness = 1)
        {
            spriteBatch.Draw(TextureAssets.MagicPixel.Value, pos, new Rectangle(0, 0, 1, 1), color, angle, Vector2.Zero, new Vector2(length, thickness), SpriteEffects.None, 0);
        }
        public static void DrawRect(this SpriteBatch spriteBatch, Rectangle rect, Color? stroke = null, Color? fill = null, float strokeThickness = 1)
        {
            if (fill.HasValue)
            {
                Main.spriteBatch.Draw(TextureAssets.MagicPixel.Value, rect, fill.Value);
            }
            if (stroke.HasValue)
            {
                spriteBatch.DrawLine(new(rect.X, rect.Y), 0, rect.Width - strokeThickness, stroke.Value, strokeThickness);
                spriteBatch.DrawLine(new(rect.X + 1, rect.Y), (float)Math.PI / 2, rect.Height - strokeThickness, stroke.Value, strokeThickness);

                spriteBatch.DrawLine(new(rect.X, rect.Y + rect.Height - strokeThickness), 0, rect.Width, stroke.Value, strokeThickness);
                spriteBatch.DrawLine(new(rect.X + rect.Width - strokeThickness + 1, rect.Y), (float)Math.PI / 2, rect.Height, stroke.Value, strokeThickness);
            }
        }
    }
}
