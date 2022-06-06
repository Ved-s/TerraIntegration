using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TerraIntegration.Basic;
using TerraIntegration.Basic.References;
using TerraIntegration.DataStructures;
using TerraIntegration.Interfaces;
using TerraIntegration.Interfaces.Math;
using TerraIntegration.UI;
using TerraIntegration.Values;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader;

namespace TerraIntegration.Variables.Numeric
{
    public class Subtract : DoubleReferenceVariableWithConst
    {
        public override string TypeName => "sub";
        public override string TypeDefaultDisplayName => "Subtract";

        public override SpriteSheetPos SpriteSheetPos => new(MathSheet, 3, 0);

        public override Type[] LeftSlotValueTypes => new[] { typeof(ISubtractable) };

        public override Type[] GetValidRightReferenceSlotTypes(Type leftSlotType)
        {
            if (VariableValue.ByType.TryGetValue(leftSlotType, out VariableValue value) && value is ISubtractable subtractable)
                return subtractable.ValidSubtractTypes;
            return null;
        }
        public override Type[] GetValidRightConstantSlotTypes(Type leftSlotType) => new[] { leftSlotType };

        public override VariableValue GetValue(ComponentSystem system, VariableValue left, VariableValue right, List<Error> errors)
        {
            ISubtractable subtractable = (ISubtractable)left;

            VariableValue result = subtractable.Subtract(right, errors);

            if (result is not null)
                SetReturnTypeCache(result.GetType());
            return result;
        }
        public override DoubleReferenceVariableWithConst CreateVariable(Variable left, ValueOrRef right)
        {
            return new Subtract()
            {
                VariableReturnType = left.VariableReturnType
            };
        }
    }
}
