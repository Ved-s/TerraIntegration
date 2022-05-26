using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TerraIntegration.Interfaces;
using TerraIntegration.Interfaces.Math;
using TerraIntegration.UI;
using TerraIntegration.Values;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader;

namespace TerraIntegration.Variables.Numeric
{
    public class Subtract : DoubleReferenceVariable
    {
        public override string Type => "sub";
        public override string TypeDisplay => "Subtract";

        public override SpriteSheetPos SpriteSheetPos => new(MathSheet, 3, 0);

        public override Type[] LeftSlotValueTypes => new[] { typeof(ISubtractable) };

        public override UIDrawing CenterDrawing => new UIDrawing()
        {
            OnDraw = (e, sb, style) =>
            {
                VariableRenderer.DrawVariableOverlay(sb, false, null, Type, style.Position() - new Vector2(16), new(32), Color.White, 0f, Vector2.Zero);
            }
        };

        public Subtract() { }
        public Subtract(Guid left, Guid right)
        {
            LeftId = left;
            RightId = right;
        }
        public override Type[] GetValidRightSlotTypes(Type leftSlotType)
        {
            if (VariableValue.ByType.TryGetValue(leftSlotType, out VariableValue value) && value is ISubtractable subtractable)
                return subtractable.ValidSubtractTypes;
            return null;
        }
        public override VariableValue GetValue(ComponentSystem system, VariableValue left, VariableValue right, List<Error> errors)
        {
            ISubtractable subtractable = (ISubtractable)left;

            VariableValue result = subtractable.Subtract(right, errors);

            if (result is not null)
                SetReturnTypeCache(result.GetType());
            return result;
        }
        public override DoubleReferenceVariable CreateVariable(Variable left, Variable right)
        {
            return new Subtract()
            {
                VariableReturnType = left.VariableReturnType
            };
        }
    }
}
