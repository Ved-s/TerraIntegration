using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerraIntegration.Basic;
using Terraria;
using Terraria.GameContent;
using Terraria.UI.Chat;

namespace TerraIntegration.DisplayedValues
{
    public class ColorTextDisplay : DisplayedValue
    {
        public override string Type => "text";

        public virtual string Text { get; }
        public virtual Color Color { get; }

        public virtual Vector2 TextAlign { get; set; } = new(.5f);

        public override string HoverText => Color == Color.White ? Text : Util.ColorTag(Color, Text);

        public ColorTextDisplay() { }
        public ColorTextDisplay(string text, Color color)
        {
            Text = text;
            Color = color;
        }

        public override void Draw(Rectangle screenRect, SpriteBatch spriteBatch)
        {
            string text = Text;
            TextSnippet[] snippets = ChatManager.ParseMessage(text, Color).ToArray();
            ChatManager.ConvertNormalSnippets(snippets);
            Vector2 size = ChatManager.GetStringSize(FontAssets.MouseText.Value, snippets, Vector2.One);

            size.Y = (text.Count(c => c == '\n') + 1) * FontAssets.MouseText.Value.LineSpacing;

            float zoomH = screenRect.Width / size.X;
            float zoomV = screenRect.Height / size.Y;

            float zoom = Math.Min(zoomH, zoomV);

            if (zoom < 1) size *= zoom;
            else zoom = 1f;

            Vector2 pos = screenRect.Location.ToVector2() + (screenRect.Size() - size) * TextAlign;
            pos.Y += 4 * zoom;

            
            ChatManager.DrawColorCodedString(spriteBatch, FontAssets.MouseText.Value, snippets, pos, Color.White, 0f, Vector2.Zero, new(zoom), out _, 0);
        }

        protected override void SendCustomData(BinaryWriter writer)
        {
            writer.Write(Text ?? "");
            writer.Write(Color.PackedValue);
        }

        protected override DisplayedValue ReceiveCustomData(BinaryReader reader)
        {
            return new ColorTextDisplay(reader.ReadString(), new Color() { PackedValue = reader.ReadUInt32() });
        }

        public override bool Equals(DisplayedValue value)
        {
            return value is ColorTextDisplay display
                   && Text == display.Text
                   && Color == display.Color;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Type, Text, Color);
        }
    }
}
