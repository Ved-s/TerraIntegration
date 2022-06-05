using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.UI;

namespace TerraIntegration.UI
{
    public class UIDrawing : UIElement
    {
        public Action<UIDrawing, SpriteBatch, CalculatedStyle> OnDraw;
        public string HoverText;

        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            OnDraw?.Invoke(this, spriteBatch, GetDimensions());
        }
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            if (IsMouseHovering && !HoverText.IsNullEmptyOrWhitespace() && ComponentWorld.Instance.HoverText is null)
                ComponentWorld.Instance.HoverText = HoverText;
        }
    }
}
