using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerraIntegration.Utilities;
using Terraria.GameContent;
using Terraria.UI.Chat;

namespace TerraIntegration.DataStructures
{
    public class FloatingText
    {
        public static List<FloatingText> Texts { get; } = new();

        public Guid Id { get; } = Guid.NewGuid();

        public TextSnippet[] Text { get; }
        public Rectf Hitbox { get => hitbox; private set => hitbox = value; }

        public Vector2 Velocity { get; set; }

        public int MaxTime { get; }
        public int Time { get; private set; }

        Color[] TextColors;
        private Rectf hitbox;

        private FloatingText(string text, Color color, int time, Vector2 velocity, Vector2 spawnPos, Vector2? spawnAlign = null)
        {
            Text = ChatManager.ParseMessage(text, color)
                .Select(t => 
                {
                    if (t.GetType() != typeof(TextSnippet))
                        return t;

                    return new TransparentText(t);
                })
                .ToArray();
            TextColors = Text.Select(s => s.Color).ToArray();
            Velocity = velocity;

            if (!spawnAlign.HasValue)
                spawnAlign = new(.5f, 1);

            Vector2 size = ChatManager.GetStringSize(FontAssets.MouseText.Value, Text, Vector2.One);
            Hitbox = new(spawnPos - size * spawnAlign.Value, size);

            MaxTime = time;
            Time = time;
        }

        private void UpdateSelf()
        {
            Vector2 velocity = Velocity;
            float centerY = Hitbox.Center.Y;

            foreach (FloatingText text in Texts)
                if (text.Id != Id && Hitbox.Intersects(text.Hitbox) && text.Hitbox.Center.Y > centerY)
                {
                    velocity *= 2;
                    break;
                }

            hitbox.Position += velocity;

            float time = (float)Time / MaxTime;

            for (int i = 0; i < Text.Length; i++)
                Text[i].Color = TextColors[i] * time;

            Time--;
        }

        private void DrawSelf(SpriteBatch batch)
        {
            ChatManager.DrawColorCodedString(batch, FontAssets.MouseText.Value, Text, Hitbox.Position, Color.White, 0, Vector2.Zero, Vector2.One, out _, -1);
        }

        public static void Update()
        {
            foreach (FloatingText text in Texts)
                text.UpdateSelf();

            Texts.RemoveAll(t => t.Time <= 0);
        }

        public static void Draw(SpriteBatch batch)
        {
            foreach (FloatingText text in Texts)
                text.DrawSelf(batch);
        }

        public static FloatingText NewText(string text, Color color, int time, Vector2 velocity, Vector2 spawnPos, Vector2? spawnAlign = null)
        {
            FloatingText t = new(text, color, time, velocity, spawnPos, spawnAlign);
            Texts.Add(t);
            return t;
        }
    }
}
