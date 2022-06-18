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
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader;
using Terraria.UI;

namespace TerraIntegration.UI
{
    public class UIComponentVariableDefinition : UIPanel
    {
        public ComponentProperty Property { get; }
        public PositionedComponent Component { get; }

        public Action<Items.Variable> DefineVariable;

        private UITextPanel TextName;
        private UIVariableSlot Slot;
        List<Error> Errors = new();

        public UIComponentVariableDefinition(ComponentProperty property, PositionedComponent component)
        {
            Property = property;
            Component = component;

            Height = new(58, 0);
            MinWidth = new(200, 0);
            PaddingTop = 0;
            PaddingBottom = 0;

            TextName = new(property.TypeDisplayName)
            {
                Left = new(44, 0),
                Width = new(-82, 1),
                Height = new(0, 1),

                BackgroundColor = Color.Transparent,
                BorderColor = Color.Transparent,

                TextAlign = new(0, .5f),

                PaddingTop = 8,
                PaddingBottom = 8,
                PaddingLeft = 0,
                PaddingRight = 0,
            };
            Append(TextName);

            Slot = new()
            {
                Top = new(-21, 0.5f),
                Left = new(-38, 1),
                AcceptEmpty = true
            };
            Append(Slot);

            Slot.VariableChanged += Slot_VariableChanged;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (Component.Component is null || Property is null) return;

            Errors.Clear();
            ComponentSystem system = null;//Component.GetData().System;
            string value = Property.GetProperty(Component, Errors)?.Display(system)?.HoverText;

            if (value is null)
                TextName.Text = Property.TypeDisplayName;
            else 
                TextName.Text = Property.TypeDisplayName + "\n[c/aaaaaa:Value:] " + value.Replace('\n', ' ');
        }

        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            base.DrawSelf(spriteBatch);

            if (Property is null) 
                return;

            const float size = 40;

            CalculatedStyle dim = GetDimensions();

            Vector2 pos = new Vector2(8, (dim.Height - size) / 2);
            pos += dim.Position();

            VariableRenderer.DrawVariableOverlay(spriteBatch, true, Property.VariableReturnType?.Type, Property.TypeName, pos, new(size), Color.White, 0f, Vector2.Zero);

            Rectangle hitbox = new((int)pos.X, (int)pos.Y, (int)size, (int)size);
            if (hitbox.Contains(Main.MouseScreen.ToPoint())) 
            {
                string returns = VariableValue.TypeToName(Property.VariableReturnType?.Type, true);

                ComponentWorld.Instance.AddHoverText(
                    $"[c/aaaa00:Type:] {Property.TypeDisplayName}" +
                    (returns is null ? "" : $"\n[c/aaaa00:Returns:] {returns}") +
                    (Property.TypeDescription is null ? "" : "\n" + Property.TypeDescription));
            }
        }

        private void Slot_VariableChanged(Items.Variable var)
        {
            if (var is null) return;
            DefineVariable?.Invoke(var);
        }
    }
}
