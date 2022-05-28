using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Linq;
using System.Text;
using TerraIntegration.DataStructures;
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
        public ValueMatcher VariableReturnTypes { get; set; } = ValueMatcher.MatchNone;
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
                if (VariableTypes is not null && !VariableTypes.Contains(var.Type))
                    return false;

                if (var.VariableReturnType is null)
                    return false;
                if (!VariableReturnTypes.Match(var.VariableReturnType))
                    return false;

                return true;
            };
        }

        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            base.DrawSelf(spriteBatch);

            ComponentWorld world = ModContent.GetInstance<ComponentWorld>();
            const float size = 40;

            CalculatedStyle dim = GetDimensions();

            Vector2 pos = new Vector2(8, (dim.Height - size) / 2);
            pos += dim.Position();

            uint change = world.UpdateCounter / 60;

            if (VariableTypes is not null)
                VariableRenderer.DrawVariableOverlay(spriteBatch, true, VariableTypes[change % VariableTypes.Length], pos, new(size), Color.White, 0f, Vector2.Zero);
            else if (VariableReturnTypes.MatchTypes is not null)
            {
                Type[] types = VariableReturnTypes.MatchTypes;
                VariableRenderer.DrawVariableOverlay(spriteBatch, true, types[change % types.Length], pos, new(size), Color.White, 0f, Vector2.Zero);
            }
            else
                VariableRenderer.DrawVariable(spriteBatch, pos, new(size), Color.White, 0f, Vector2.Zero);

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
                            return var.TypeDisplay;
                        }
                        return $"Unregistered ({t})";
                    })));
                }
                if (VariableReturnTypes.MatchTypes is not null)
                {
                    hover.Append("[c/aaaa00:");
                    hover.Append("Returns");
                    hover.Append(":] ");
                    hover.AppendLine(string.Join(", ", VariableReturnTypes.MatchTypes.Select(t => VariableValue.TypeToName(t, true))));
                }
                if (VariableReturnTypes.MatchesNone && VariableTypes is null) hover.AppendLine("Accepts any variable");

                if (!VariableDescription.IsNullEmptyOrWhitespace())
                {
                    hover.AppendLine();
                    hover.Append(VariableDescription);
                }

                ModContent.GetInstance<ComponentWorld>().HoverText = hover.ToString();
            }
        }
    }
}
