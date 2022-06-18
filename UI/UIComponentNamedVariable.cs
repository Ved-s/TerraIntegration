using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerraIntegration.Basic;
using TerraIntegration.Utilities;
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
        public string[] VariableTypes { get; set; } = null;
        public Type[] VariableReturnTypes { get; set; } = null;
        public string VariableDescription { get; set; }

        public Action<Items.Variable> DefineVariable;

        public PositionedComponent Component
        {
            get => Slot.Component;
            set => Slot.Component = value;
        }
        public string VariableSlot
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

            MinWidth = new(200, 0);

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
                if (VariableTypes is not null && !VariableTypes.Contains(var.TypeName))
                    return false;

                if (VariableReturnTypes is not null)
                {
                    if (var.VariableReturnType is null)
                        return false;
                    if (!VariableReturnTypes.Any(t => var.VariableReturnType.Value.Match(t)))
                        return false;
                }

                return true;
            };
        }

        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            base.DrawSelf(spriteBatch);

            ComponentWorld world = ComponentWorld.Instance;
            const float size = 40;

            CalculatedStyle dim = GetDimensions();

            Vector2 pos = new Vector2(8, (dim.Height - size) / 2);
            pos += dim.Position();

            uint change = world.UpdateCounter / 60;

            if (VariableTypes is not null)
                VariableRenderer.DrawVariableOverlay(spriteBatch, true, null, VariableTypes[change % VariableTypes.Length], pos, new(size), Color.White, 0f, Vector2.Zero);
            else if (VariableReturnTypes is not null)
                VariableRenderer.DrawVariableOverlay(spriteBatch, true, VariableReturnTypes[change % VariableReturnTypes.Length], null, pos, new(size), Color.White, 0f, Vector2.Zero);
            else
                VariableRenderer.DrawVariableOverlay(spriteBatch, true, null, null, pos, new(size), Color.White, 0f, Vector2.Zero);

            Rectangle hitbox = new((int)pos.X, (int)pos.Y, (int)size, (int)size);
            if (hitbox.Contains(Main.MouseScreen.ToPoint()))
            {
                StringBuilder hover = new();

                if (VariableTypes is not null)
                {
                    hover.Append("[c/aaaa00:");
                    hover.Append(VariableTypes.Length == 1 ? "Type" : "Types");
                    hover.Append(":] ");
                    hover.AppendLine(string.Join(", ", VariableTypes.Select(t =>
                    {
                        if (Variable.ByTypeName.TryGetValue(t, out Variable var))
                        {
                            return var.TypeDisplayName;
                        }
                        return $"Unregistered ({t})";
                    })));
                }
                if (VariableReturnTypes is not null)
                {
                    hover.Append("[c/aaaa00:");
                    hover.Append("Returns");
                    hover.Append(":] ");
                    hover.AppendLine(string.Join(", ", VariableReturnTypes.Select(t => VariableValue.TypeToName(t, true))));
                }
                if (VariableReturnTypes is null && VariableTypes is null) hover.AppendLine("Accepts any variable");

                if (!VariableDescription.IsNullEmptyOrWhitespace())
                {
                    hover.Append(VariableDescription);
                }

                ComponentWorld.Instance.AddHoverText(hover.ToString());
            }
        }
    }
}
