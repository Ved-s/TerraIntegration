using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerraIntegration.Basic;
using TerraIntegration.DataStructures;
using TerraIntegration.Utilities;
using TerraIntegration.Values;
using TerraIntegration.Variables;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;

namespace TerraIntegration.UI
{
    public class UIVariableSwitch : UIElement
    {
        public ValueVariablePair[] SwitchValues { get; set; }

        public int Index
        {
            get => index;
            set
            {
                bool changed = index != value;
                index = value;

                if (changed)
                    CurrentValueChanged?.Invoke(Current);
            }
        }
        public ValueVariablePair? Current => (Index < 0 || Index >= (SwitchValues?.Length ?? 0)) ? null : SwitchValues[Index];

        public Action<ValueVariablePair?> CurrentValueChanged;
        private int index = 0;

        public UIVariableSwitch()
        {
            Width = new(32, 0);
            Height = new(32, 0);
        }

        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            if (Current is null && SwitchValues?.Length is not null and > 0)
                Index = 0;

            ValueVariablePair? pair = Current;

            if (pair is not null && pair.Value.AllowVarClick && pair.Value.ValueType is null)
            {
                int ind = SwitchValues.IndexOf(v => !v.AllowVarClick || v.ValueType.HasValue);
                if (ind >= 0)
                    Index = ind;
            }

            CalculatedStyle style = GetDimensions();

            VariableRenderer.DrawVariableOverlay(spriteBatch, true, pair?.ValueType?.Type, pair?.VariableType, style.Position(), style.Size(), Color.White, 0f, Vector2.Zero);
        }

        public override void Update(GameTime gameTime)
        {
            if (IsMouseHovering)
            {
                ValueVariablePair? cur = Current;

                if (!cur.HasValue) return;

                string variable =
                    cur.Value.VariableType is not null
                    && Variable.ByTypeName.TryGetValue(cur.Value.VariableType, out Variable var) ?
                    var.TypeDisplayName : null;

                string value = cur.Value.ValueType is not null ? cur.Value.ValueType.Value.ToStringName(true) : null;

                if (variable is not null || value is not null || cur.Value.HoverText is not null)
                {
                    string hover;

                    if (cur.Value.HoverText is not null)
                        hover = cur.Value.HoverText;
                    else if (variable is not null && value is not null)
                        hover = $"{variable} ({value})";
                    else if (variable is not null)
                        hover = variable;
                    else
                        hover = value;

                    hover += "\nClick to switch types";

                    if (SwitchValues.Any(v => v.AllowVarClick))
                        hover += "\nClick with variable to set type";

                    ComponentWorld.Instance.AddHoverText(hover);
                }
            }
        }

        public override void Click(UIMouseEvent evt)
        {
            base.Click(evt);

            if (Main.mouseItem?.ModItem is Items.Variable var && SwitchValues.Any(v => v.AllowVarClick) && var.Var?.VariableReturnType is not null)
            {
                int freeIndex = SwitchValues.IndexOf(v => v.AllowVarClick && !v.ValueType.HasValue && (v.VariableType is null || v.VariableType == var.Var.TypeName));
                if (freeIndex < 0)
                    freeIndex = SwitchValues.IndexOf(v => v.AllowVarClick && (v.VariableType is null || v.VariableType == var.Var.TypeName));

                if (freeIndex >= 0)
                {
                    ValueVariablePair pair = SwitchValues[freeIndex];
                    pair.ValueType = var.Var.VariableReturnType;
                    SwitchValues[freeIndex] = pair;
                    Index = freeIndex;
                    return;
                }
            }

            if (SwitchValues?.Length is not null and > 1)
            {
                Index = (Index + 1) % SwitchValues.Length;
                SoundEngine.PlaySound(SoundID.MenuTick);
            }
        }
        public override void RightClick(UIMouseEvent evt)
        {
            base.RightClick(evt);

            if (SwitchValues?.Length is not null and > 1)
            {
                int index = Index - 1;
                if (index < 0)
                    index = SwitchValues.Length - 1;
                Index = index;

                SoundEngine.PlaySound(SoundID.MenuTick);
            }
        }
    }

    public record struct ValueVariablePair(ReturnType? ValueType, string VariableType, string HoverText = null, bool AllowVarClick = false);
}
