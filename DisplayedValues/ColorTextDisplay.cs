﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        public override string HoverText => Color == Color.White ? Text : Util.ColorTag(Color, Text);

        public ColorTextDisplay() { }
        public ColorTextDisplay(string text, Color color)
        {
            Text = text;
            Color = color;
        }

        public override void Draw(Rectangle screenRect, SpriteBatch spriteBatch)
        {
            Vector2 size = FontAssets.MouseText.Value.MeasureString(Text);

            float zoomH = screenRect.Width / size.X;
            float zoomV = screenRect.Height / size.Y;

            float zoom = Math.Min(zoomH, zoomV);

            if (zoom < 1) size *= zoom;
            else zoom = 1f;

            Vector2 pos = screenRect.Location.ToVector2() + (screenRect.Size() / 2 - size / 2);
            pos.Y += 4 * zoom;

            TextSnippet[] snippets = ChatManager.ParseMessage(Text, Color).ToArray();
            ChatManager.ConvertNormalSnippets(snippets);
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

        public override bool Equals(object obj)
        {
            return obj is ColorTextDisplay display &&
                   Type == display.Type &&
                   Text == display.Text &&
                   Color.Equals(display.Color);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Type, Text, Color);
        }
    }
}