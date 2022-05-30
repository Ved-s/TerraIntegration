using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerraIntegration.Basic;
using TerraIntegration.Values;
using TerraIntegration.Variables;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;

namespace TerraIntegration.UI
{
    public class UIVariableSwitch : UIElement
    {
        public Type[] SwitchValueTypes { get; set; }
        public string[] SwitchVariableTypes { get; set; }

        int ValueIndex = 0;
        int VariableIndex = 0;

        public Type CurrentValueType => (ValueIndex < 0 || ValueIndex >= (SwitchValueTypes?.Length ?? 0)) ? null : SwitchValueTypes[ValueIndex];
        public string CurrentVariableType => (VariableIndex < 0 || VariableIndex >= (SwitchVariableTypes?.Length ?? 0)) ? null : SwitchVariableTypes[VariableIndex];

        public UIVariableSwitch() 
        {
            Width = new(32, 0);
            Height = new(32, 0);
        }

        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            if (CurrentValueType is null && SwitchValueTypes?.Length is not null and > 0)
                ValueIndex = 0;

            if (CurrentVariableType is null && SwitchVariableTypes?.Length is not null and > 0)
                VariableIndex = 0;

            CalculatedStyle style = GetDimensions();

            VariableRenderer.DrawVariableOverlay(spriteBatch, true, CurrentValueType, CurrentVariableType, style.Position(), style.Size(), Color.White, 0f, Vector2.Zero);
        }

        public override void Update(GameTime gameTime)
        {
            if (IsMouseHovering)
            {
                string variable =
                    CurrentVariableType is not null
                    && Variable.ByTypeName.TryGetValue(CurrentVariableType, out Variable var) ?
                    var.TypeDisplay : null;

                string value = CurrentValueType is not null ? VariableValue.TypeToName(CurrentValueType, true) : null;

                if (variable is not null || value is not null)
                {
                    string hover;

                    if (variable is not null && value is not null)
                        hover = $"{variable} ({value})";
                    else if (variable is not null)
                        hover = variable;
                    else
                        hover = value;

                    ModContent.GetInstance<ComponentWorld>().HoverText = hover;
                }
            }
        }

        public override void Click(UIMouseEvent evt)
        {
            base.Click(evt);

            if (SwitchVariableTypes?.Length is not null and > 1)
            {
                VariableIndex = (VariableIndex + 1) % SwitchVariableTypes.Length;
                SoundEngine.PlaySound(SoundID.MenuTick);
            }

            else if (SwitchValueTypes?.Length is not null and > 1)
            {
                ValueIndex = (ValueIndex + 1) % SwitchValueTypes.Length;
                SoundEngine.PlaySound(SoundID.MenuTick);
            }
        }
        public override void RightClick(UIMouseEvent evt)
        {
            base.RightClick(evt);

            if (SwitchValueTypes?.Length is not null and > 1)
            {
                ValueIndex = (ValueIndex + 1) % SwitchValueTypes.Length;
                SoundEngine.PlaySound(SoundID.MenuTick);
            }

            else if (SwitchVariableTypes?.Length is not null and > 1)
            {
                VariableIndex = (VariableIndex + 1) % SwitchVariableTypes.Length;
                SoundEngine.PlaySound(SoundID.MenuTick);
            }
        }
    }
}
