using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.UI;

namespace TerraIntegration.UI
{
    public class UISwitch : UIElement
    {
        public SwitchState[] States { get; set; }
        public int StateIndex 
        {
            get => stateIndex;
            set
            {
                stateIndex = value;
                if (States?.Length is not null and > 0)
                    StateChanged?.Invoke(CurrentState.Value, value);
            }
        }

        public SwitchState? CurrentState
        {
            get
            {
                if (States?.Length is null or 0)
                    return null;
                return States[PositiveMod(StateIndex, States.Length)];
            }
        }
        public Action<SwitchState, int> StateChanged;
        private int stateIndex;

        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            base.DrawSelf(spriteBatch);

            SwitchState? state = CurrentState;
            if (state?.Texture is null) return;

            Vector2 texSize = state.Value.Frame.HasValue
                ? state.Value.Frame.Value.Size()
                : state.Value.Texture.Size();

            CalculatedStyle dim = GetDimensions();

            float scale = Math.Min(dim.Width / texSize.X, dim.Height / texSize.Y);
            Vector2 pos = (dim.Size() - texSize * scale) / 2 + dim.Position();

            spriteBatch.Draw(state.Value.Texture, pos, state.Value.Frame, Color.White);
        }
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (IsMouseHovering)
            {
                SwitchState? state = CurrentState;
                if (state?.HoverText.IsNullEmptyOrWhitespace() ?? true) return;

                ComponentWorld.Instance.AddHoverText(state.Value.HoverText);
            }
        }

        public override void Click(UIMouseEvent evt)
        {
            base.Click(evt);
            if (States?.Length is null or 0) return;
            StateIndex = PositiveMod(StateIndex - 1, States.Length);
            SoundEngine.PlaySound(SoundID.MenuTick);
        }
        public override void RightClick(UIMouseEvent evt)
        {
            base.RightClick(evt);
            if (States?.Length is null or 0) return;
            StateIndex = PositiveMod(StateIndex + 1, States.Length);
            SoundEngine.PlaySound(SoundID.MenuTick);
        }

        int PositiveMod(int value, int mod)
        {
            value %= mod;
            if (value < 0)
                value += mod;
            return value;
        }

        public struct SwitchState
        {
            public readonly Texture2D Texture;
            public readonly Rectangle? Frame;
            public readonly string HoverText;
            public readonly object Tag;

            public SwitchState(Texture2D texture, Rectangle? frame, string hoverText, object tag = null)
            {
                Texture = texture;
                Frame = frame;
                HoverText = hoverText;
                Tag = tag;
            }
        }
    }
}
