using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerraIntegration.Values;
using TerraIntegration.Variables;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader;
using Terraria.UI;

namespace TerraIntegration.UI
{
    public class UIComponentNamedVariable : UIPanel
    {
        public string VariableType { get; set; } = null;
        public Type VariableReturnType { get; set; } = typeof(VariableValue);
        public string VariableDescription { get; set; }

        public Action<Items.Variable> DefineVariable;

        public PositionedComponent Component
        {
            get => Slot.Component;
            set => Slot.Component = value;
        }
        public int VariableSlot
        {
            get => Slot.VariableSlot;
            set => Slot.VariableSlot = value;
        }
        public string VariableName
        {
            get => variableName;
            set
            {
                variableName = value;
                TextName.SetText(variableName);
            }
        }

        private UIText TextName;
        private UIComponentVariable Slot;
        private string variableName;

        public UIComponentNamedVariable()
        {
            Width = new(0, 1);
            Height = new(58, 0);

            TextName = new("unnamed variable")
            {
                Left = new(44, 0),
                Width = new(-82, 1),
                Height = new(0, 1),
                MarginTop = 10,
                TextOriginX = 0
            };
            Append(TextName);

            Slot = new()
            {
                Top = new(-21, 0.5f),
                Left = new(-38, 1),
            };
            Append(Slot);

            Slot.VariableValidator = (var) =>
            {
                if (VariableType is not null && var.Type != VariableType)
                    return false;

                if (VariableReturnType is not null)
                {
                    if (var.VariableReturnType is null)
                        return false;
                    if (!var.VariableReturnType.IsSubclassOf(VariableReturnType))
                        return false;
                }

                return true;
            };
        }

        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            base.DrawSelf(spriteBatch);

            const float size = 40;

            CalculatedStyle dim = GetDimensions();

            Vector2 pos = new Vector2(8, (dim.Height - size) / 2);
            pos += dim.Position();

            VariableRenderer.DrawVariableOverlay(spriteBatch, true, VariableReturnType, VariableType, pos, new(size), Color.White, 0f, Vector2.Zero);

            Rectangle hitbox = new((int)pos.X, (int)pos.Y, (int)size, (int)size);
            if (hitbox.Contains(Main.MouseScreen.ToPoint()))
            {
                string returns;

                if (VariableReturnType is null) returns = null;
                else if (Values.VariableValue.ByType.TryGetValue(VariableReturnType, out Values.VariableValue val))
                {
                    returns = Util.ColorTag(val.TypeColor, val.TypeDisplay);
                }
                else if (VariableReturnType.IsInterface)
                {
                    string i = VariableReturnType.Name;
                    if (i.StartsWith('I')) i = i[1..];

                    returns = $"[c/aabb00:{i}]";
                }
                else returns = $"[c/ffaaaa:unregistered type ({VariableReturnType.Name})]";

                string typeDisplay = null;

                if (Variable.ByTypeName.TryGetValue(VariableType, out Variable var))
                {
                    typeDisplay = var.TypeDisplay;
                }
                else
                {
                    typeDisplay = $"unregistered variable ({VariableType})";
                }

                ModContent.GetInstance<ComponentWorld>().HoverText =
                    $"[c/aaaa00:Type:] {typeDisplay}" +
                    (returns is null ? "" : $"\n[c/aaaa00:Returns:] {returns}") +
                    (VariableDescription is null ? "" : "\n" + VariableDescription);
            }
        }

        private void Slot_VariableChanged()
        {
            if (Slot.Var is null) return;
            DefineVariable?.Invoke(Slot.Var);
        }
    }
}
