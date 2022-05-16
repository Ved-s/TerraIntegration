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
    public class UIComponentVariableDefinition : UIPanel
    {
        public string VariableType { get; set; } = "any";

        public Action<Items.Variable> DefineVariable;

        private UIText TextName;
        private UIVariableSlot Slot;

        public UIComponentVariableDefinition()
        {
            Height = new(58, 0);

            TextName = new("unregistered variable")
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

            Slot.VariableChanged += Slot_VariableChanged;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (!Variable.ByTypeName.TryGetValue(VariableType, out Variable var))
                return;

            TextName.SetText(var.TypeDisplay);
        }

        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            base.DrawSelf(spriteBatch);

            const float size = 40;

            if (!Variable.ByTypeName.TryGetValue(VariableType, out Variable var))
                return;

            CalculatedStyle dim = GetDimensions();

            Vector2 pos = new Vector2(8, (dim.Height - size) / 2);
            pos += dim.Position();

            VariableRenderer.DrawVariableOverlay(spriteBatch, true, var.VariableReturnType, var.Type, pos, new(size), Color.White, 0f, Vector2.Zero);

            Rectangle hitbox = new((int)pos.X, (int)pos.Y, (int)size, (int)size);
            if (hitbox.Contains(Main.MouseScreen.ToPoint())) 
            {
                string returns;

                if (var.VariableReturnType is null) returns = null;
                else if (Values.VariableValue.ByType.TryGetValue(var.VariableReturnType, out Values.VariableValue val))
                {
                    returns = Util.ColorTag(val.TypeColor, val.TypeDisplay);
                }
                else if (var.VariableReturnType?.IsInterface ?? false)
                {
                    string i = var.VariableReturnType.Name;
                    if (i.StartsWith('I')) i = i[1..];

                    returns = $"[c/aabb00:{i}]";
                }
                else returns = $"[c/ffaaaa:unregistered type ({var.VariableReturnType.Name})]";

                ModContent.GetInstance<ComponentWorld>().HoverText =
                    $"[c/aaaa00:Type:] {var.TypeDisplay}" +
                    (returns is null ? "" : $"\n[c/aaaa00:Returns:] {returns}") +
                    (var.TypeDescription is null ? "" : "\n" + var.TypeDescription);
            }
        }

        private void Slot_VariableChanged()
        {
            if (Slot.Var is null) return;
            DefineVariable?.Invoke(Slot.Var);
        }
    }
}
