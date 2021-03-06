using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TerraIntegration.Basic;
using TerraIntegration.DataStructures;
using TerraIntegration.Interfaces;
using TerraIntegration.Interfaces.Value.Math;
using TerraIntegration.Templates;
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
        public override string TypeDefaultDescription => "Subtracts right value from left value";

        public override SpriteSheetPos SpriteSheetPos => new(MathSheet, 3, 0);

        public override ReturnType[] LeftSlotValueTypes => new ReturnType[] { typeof(ISubtractable) };

        public override ReturnType[] GetValidRightReferenceSlotTypes(ReturnType leftSlotType)
        {
            if (VariableValue.ByType.TryGetValue(leftSlotType.Type, out VariableValue value) && value is ISubtractable subtractable)
                return subtractable.ValidSubtractTypes.Cast<ReturnType>().ToArray();
            return null;
        }
        public override Type[] GetValidRightConstantSlotTypes(ReturnType leftSlotType) => new[] { leftSlotType.Type };

        public override VariableValue GetValue(ComponentSystem system, VariableValue left, VariableValue right, List<Error> errors)
        {
            ISubtractable subtractable = (ISubtractable)left;

            VariableValue result = subtractable.Subtract(right, errors, TypeIdentity);

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
