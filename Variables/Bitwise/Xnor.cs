using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerraIntegration.Basic;
using TerraIntegration.DataStructures;
using TerraIntegration.Interfaces;

namespace TerraIntegration.Variables.Bitwise
{
    public class Xnor : DoubleReferenceVariableWithConst
    {
        public override Type[] LeftSlotValueTypes => new[] { typeof(Values.Boolean), typeof(INumeric) };
        public override string Type => "xnor";
        public override string TypeDisplay => "Xnor";

        public override SpriteSheetPos SpriteSheetPos => new(BooleanSheet, 3, 0);

        public override Type[] GetValidRightSlotTypes(Type leftSlotType)
        {
            return new[] { leftSlotType };
        }

        public override DoubleReferenceVariableWithConst CreateVariable(Variable left, ValueOrRef right)
        {
            return new Xnor
            {
                VariableReturnType = left.VariableReturnType
            };
        }

        public override VariableValue GetValue(ComponentSystem system, VariableValue left, VariableValue right, List<Error> errors)
        {
            if (left is Values.Boolean boolean)
                return new Values.Boolean(!(boolean.Value ^ ((Values.Boolean)right).Value));

            INumeric numLeft = (INumeric)right;
            INumeric numRight = (INumeric)left;

            long bitmask = 0;
            byte bitWidth = numLeft.BitWidth;
            for (byte i = 0; i < bitWidth; i++)
                bitmask = (bitmask << 1) | 1;

            return numLeft.GetFromNumeric((numLeft.NumericValue !^ numRight.NumericValue) & bitmask, errors);
        }
    }
}
