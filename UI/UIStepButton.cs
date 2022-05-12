using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;

namespace TerraIntegration.UI
{
    public class UIStepButton : UIElement
    {
        static Texture2D StepButtonTexture;

        public int Step { get; set; } = 1;
        public int ShiftStep { get; set; } = 10;

        public event Action<int> OnStep;

        private bool UpPressed = false;
        private bool DownPressed = false;

        public UIStepButton()
        {
            Width = new(24, 0);
            Height = new(24, 0);
            if (StepButtonTexture is null)
                StepButtonTexture = ModContent.Request<Texture2D>($"{nameof(TerraIntegration)}/Assets/UI/StepButton", AssetRequestMode.ImmediateLoad).Value;
        }

        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            CalculatedStyle dim = GetDimensions();

            Rectangle frame = new(0, 0, StepButtonTexture.Width / 4, StepButtonTexture.Height);
            if (UpPressed) frame.X += frame.Width;
            if (DownPressed) frame.X += frame.Width * 2;

            Vector2 scale = new Vector2(dim.Width, dim.Height) / frame.Size();

            spriteBatch.Draw(StepButtonTexture, dim.Position(), frame, Color.White, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
        }

        public override void MouseDown(UIMouseEvent evt)
        {
            base.MouseDown(evt);

            CalculatedStyle dim = GetDimensions();
            Vector2 relative = evt.MousePosition - dim.Position();

            UpPressed = false;
            DownPressed = false;
            if (relative.Y < dim.Height / 2)
            {
                UpPressed = true;
                DownPressed = false;
            }
            else 
            {
                UpPressed = false;
                DownPressed = true;
            }
        }

        public override void MouseUp(UIMouseEvent evt)
        {
            base.MouseUp(evt);
            UpPressed = false;
            DownPressed = false;
        }

        public override void Click(UIMouseEvent evt)
        {
            CalculatedStyle dim = GetDimensions();
            Vector2 relative = evt.MousePosition - dim.Position();

            int step = Step;
            if (Main.keyState.PressingShift()) step = ShiftStep;

            if (relative.Y > dim.Height / 2) 
                step = -step;

            OnStep?.Invoke(step);
            SoundEngine.PlaySound(SoundID.MenuTick);
        }
    }
}
