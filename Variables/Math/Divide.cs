using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TerraIntegration.Basic;
using TerraIntegration.Interfaces;
using TerraIntegration.Interfaces.Math;
using TerraIntegration.UI;
using TerraIntegration.Values;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader;

namespace TerraIntegration.Variables.Numeric
{
    public class Divide : DoubleReferenceVariable
    {
        public override string Type => "div";
        public override string TypeDisplay => "Divide";

        public override SpriteSheetPos SpriteSheetPos => new(MathSheet, 1, 1);

        public override Type[] LeftSlotValueTypes => new[] { typeof(IDivisible) };

        public Divide() { }
        public Divide(Guid left, Guid right)
        {
            LeftId = left;
            RightId = right;
        }
        public override Type[] GetValidRightSlotTypes(Type leftSlotType)
        {
            if (VariableValue.ByType.TryGetValue(leftSlotType, out VariableValue value) && value is IDivisible divisible)
                return divisible.ValidDivideTypes;
            return null;
        }
        public override VariableValue GetValue(ComponentSystem system, VariableValue left, VariableValue right, List<Error> errors)
        {
            IDivisible divisible = (IDivisible)left;

            VariableValue result = divisible.Divide(right, errors);

            if (result is not null)
                SetReturnTypeCache(result.GetType());
            return result;
        }
        public override DoubleReferenceVariable CreateVariable(Variable left, Variable right)
        {
            return new Divide()
            {
                VariableReturnType = left.VariableReturnType
            };
        }
    }
}
