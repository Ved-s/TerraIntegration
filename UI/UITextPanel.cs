using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;
using Terraria.UI.Chat;

namespace TerraIntegration.UI
{
    public class UITextPanel : UIPanel
    {
        public string Text { get; set; }
        public Color Color { get; set; } = Color.White;

        public float MaxScale { get; set; } = 1f;

        public Vector2 TextAlign { get; set; } = new(.5f);

        public UITextPanel(string text = null, Color? color = null, float maxScale = 1f, Vector2? textAlign = null)
        {
            Text = text;
            if (color is not null)
                Color = color.Value;
            MaxScale = maxScale;
            if (textAlign is not null)
                TextAlign = textAlign.Value;
        }

        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            base.DrawSelf(spriteBatch);

            if (Text is null) return;

            TextSnippet[] snippets = ChatManager.ParseMessage(Text, Color).ToArray();
            Vector2 textSize = ChatManager.GetStringSize(FontAssets.MouseText.Value, snippets, new(1));

            CalculatedStyle inner = GetInnerDimensions();

            float scale = Math.Min(inner.Width / textSize.X, inner.Height / textSize.Y);
            scale = Math.Min(scale, MaxScale);

            Vector2 pos = (inner.Size() - textSize * scale) * TextAlign + inner.Position();
            pos.Y += 4 * scale;

            ChatManager.DrawColorCodedStringWithShadow(spriteBatch, FontAssets.MouseText.Value, snippets, pos, 0f, Vector2.Zero, new Vector2(scale), out _);
        }
    }
}
