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
    public class Not : ReferenceVariable
    {
        public override string Type => "not";
        public override string TypeDisplay => "Not";

        public override Type[] ReferenceReturnTypes => new[] { typeof(Values.Boolean), typeof(INumeric) };

        public override VariableValue GetValue(VariableValue value, ComponentSystem system, List<Error> errors)
        {
            if (value is Values.Boolean boolean)
                return new Values.Boolean(!boolean.Value);

            INumeric numeric = (INumeric)value;

            long bitmask = 0;
            byte bitWidth = numeric.BitWidth;
            for (byte i = 0; i < bitWidth; i++)
                bitmask = (bitmask << 1) | 1;
            
            long newValue = ~numeric.NumericValue & bitmask;

            return numeric.GetFromNumeric(newValue, errors);
        }
    }
}
