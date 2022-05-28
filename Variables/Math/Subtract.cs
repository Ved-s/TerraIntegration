using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TerraIntegration.DataStructures;
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

        public override ValueMatcher LeftSlotValueTypes => ValueMatcher.OfType<ISubtractable>();

        public Subtract() { }
        public Subtract(Guid left, Guid right)
        {
            LeftId = left;
            RightId = right;
        }
        public override ValueMatcher GetValidRightSlotTypes(ReturnValue? leftSlotReturn)
        {
            if (!leftSlotReturn.HasValue)
                return ValueMatcher.MatchNone;
            return leftSlotReturn.Value.GetInstanceInterface<ISubtractable>()?.ValidSubtractTypes ?? ValueMatcher.MatchNone;
        }
        public override VariableValue GetValue(ComponentSystem system, VariableValue left, VariableValue right, List<Error> errors)
        {
            ISubtractable subtractable = (ISubtractable)left;

            VariableValue result = subtractable.Subtract(right, errors);

            if (result is not null)
                SetReturnTypeCache(ReturnValue.OfType(result.GetType()));
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
