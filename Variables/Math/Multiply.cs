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
    public class Multiply : DoubleReferenceVariableWithConst
    {
        public override string TypeName => "mul";
        public override string TypeDefaultDisplayName => "Multiply";

        public override SpriteSheetPos SpriteSheetPos => new(MathSheet, 0, 1);

        public override Type[] LeftSlotValueTypes => new[] { typeof(IMultipliable) };

        public override Type[] GetValidRightReferenceSlotTypes(Type leftSlotType)
        {
            if (VariableValue.ByType.TryGetValue(leftSlotType, out VariableValue value) && value is IMultipliable multipliable)
                return multipliable.ValidMultiplyTypes;
            return null;
        }
        public override Type[] GetValidRightConstantSlotTypes(Type leftSlotType) => new[] { leftSlotType };

        public override VariableValue GetValue(ComponentSystem system, VariableValue left, VariableValue right, List<Error> errors)
        {
            IMultipliable multipliable = (IMultipliable)left;

            VariableValue result = multipliable.Multiply(right, errors);

            if (result is not null)
                SetReturnTypeCache(result.GetType());
            return result;
        }
        public override DoubleReferenceVariableWithConst CreateVariable(Variable left, ValueOrRef right)
        {
            return new Multiply()
            {
                VariableReturnType = left.VariableReturnType
            };
        }
    }
}
