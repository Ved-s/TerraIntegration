using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TerraIntegration.Basic;
using TerraIntegration.DataStructures;
using TerraIntegration.Interfaces;
using TerraIntegration.Interfaces.Math;
using TerraIntegration.UI;
using TerraIntegration.Values;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader;

namespace TerraIntegration.Variables.Numeric
{
    public class Divide : DoubleReferenceVariableWithConst
    {
        public override string TypeName => "div";
        public override string TypeDefaultDisplayName => "Divide";

        public override SpriteSheetPos SpriteSheetPos => new(MathSheet, 1, 1);

        public override Type[] LeftSlotValueTypes => new[] { typeof(IDivisible) };

        public override Type[] GetValidRightReferenceSlotTypes(Type leftSlotType)
        {
            if (VariableValue.ByType.TryGetValue(leftSlotType, out VariableValue value) && value is IDivisible divisible)
                return divisible.ValidDivideTypes;
            return null;
        }
        public override Type[] GetValidRightConstantSlotTypes(Type leftSlotType) => new[] { leftSlotType };

        public override VariableValue GetValue(ComponentSystem system, VariableValue left, VariableValue right, List<Error> errors)
        {
            IDivisible divisible = (IDivisible)left;

            VariableValue result = divisible.Divide(right, errors);

            if (result is not null)
                SetReturnTypeCache(result.GetType());
            return result;
        }
        public override DoubleReferenceVariableWithConst CreateVariable(Variable left, ValueOrRef right)
        {
            return new Divide()
            {
                VariableReturnType = left.VariableReturnType
            };
        }
    }
}
