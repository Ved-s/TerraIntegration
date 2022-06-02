﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerraIntegration.Basic;
using TerraIntegration.DataStructures;
using TerraIntegration.Interfaces;

namespace TerraIntegration.Variables.Bitwise
{
    public class Xor : DoubleReferenceVariableWithConst
    {
        public override Type[] LeftSlotValueTypes => new[] { typeof(Values.Boolean), typeof(INumeric) };
        public override string Type => "xor";
        public override string TypeDisplay => "Xor";

        public override Type[] GetValidRightSlotTypes(Type leftSlotType)
        {
            return new[] { leftSlotType };
        }

        public override DoubleReferenceVariableWithConst CreateVariable(Variable left, ValueOrRef right)
        {
            return new Xor
            {
                VariableReturnType = left.VariableReturnType
            };
        }

        public override VariableValue GetValue(ComponentSystem system, VariableValue left, VariableValue right, List<Error> errors)
        {
            if (left is Values.Boolean boolean)
                return new Values.Boolean(boolean.Value ^ ((Values.Boolean)right).Value);

            INumeric numLeft = (INumeric)right;
            INumeric numRight = (INumeric)left;

            return numLeft.GetFromNumeric(numLeft.NumericValue ^ numRight.NumericValue, errors);
        }
    }
}
