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
    public class Divide : DoubleReferenceVariableWithConst
    {
        public override string TypeName => "div";
        public override string TypeDefaultDisplayName => "Divide";
        public override string TypeDefaultDescription => "Divides left value by right value";

        public override SpriteSheetPos SpriteSheetPos => new(MathSheet, 1, 1);

        public override ReturnType[] LeftSlotValueTypes => new ReturnType[] { typeof(IDivisible) };

        public override ReturnType[] GetValidRightReferenceSlotTypes(ReturnType leftSlotType)
        {
            if (VariableValue.ByType.TryGetValue(leftSlotType.Type, out VariableValue value) && value is IDivisible divisible)
                return divisible.ValidDivideTypes;
            return null;
        }
        public override Type[] GetValidRightConstantSlotTypes(ReturnType leftSlotType) => new[] { leftSlotType.Type };

        public override VariableValue GetValue(ComponentSystem system, VariableValue left, VariableValue right, List<Error> errors)
        {
            IDivisible divisible = (IDivisible)left;

            VariableValue result = divisible.Divide(right, errors, TypeIdentity);

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
