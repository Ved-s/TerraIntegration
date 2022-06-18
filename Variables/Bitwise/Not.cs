using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerraIntegration.Basic;
using TerraIntegration.DataStructures;
using TerraIntegration.Interfaces;
using TerraIntegration.Templates;

namespace TerraIntegration.Variables.Bitwise
{
    public class Not : ReferenceVariable
    {
        public override string TypeName => "not";
        public override string TypeDefaultDisplayName => "Not";

        public override SpriteSheetPos SpriteSheetPos => new(BooleanSheet, 2, 0);
        public override string TypeDefaultDescription => "Boolean or bitwise Not operator";

        public override ReturnType[] ReferenceReturnTypes => new ReturnType[] { typeof(Values.Boolean), typeof(INumeric) };

        public override ReferenceVariable CreateVariable(Variable var)
        {
            return new Not
            {
                VariableReturnType = var.VariableReturnType
            };
        }

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
