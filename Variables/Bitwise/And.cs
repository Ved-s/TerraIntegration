using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerraIntegration.Basic;
using TerraIntegration.Basic.References;
using TerraIntegration.DataStructures;
using TerraIntegration.Interfaces;

namespace TerraIntegration.Variables.Bitwise
{
    public class And : DoubleReferenceVariableWithConst
    {
        public override Type[] LeftSlotValueTypes => new[] { typeof(Values.Boolean), typeof(INumeric) };
        public override string TypeName => "and";
        public override string TypeDefaultDisplayName => "And";

        public override SpriteSheetPos SpriteSheetPos => new(BooleanSheet, 0, 0);

        public override Type[] GetValidRightSlotTypes(Type leftSlotType)
        {
            return new[] { leftSlotType };
        }

        public override DoubleReferenceVariableWithConst CreateVariable(Variable left, ValueOrRef right)
        {
            return new And
            {
                VariableReturnType = left.VariableReturnType
            };
        }

        public override VariableValue GetValue(ComponentSystem system, VariableValue left, VariableValue right, List<Error> errors)
        {
            if (left is Values.Boolean boolean)
                return new Values.Boolean(boolean.Value && ((Values.Boolean)right).Value);

            INumeric numLeft = (INumeric)right;
            INumeric numRight = (INumeric)left;

            return numLeft.GetFromNumeric(numLeft.NumericValue & numRight.NumericValue, errors);
        }
    }
}
