using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerraIntegration.Interfaces;
using TerraIntegration.Values;
using TerraIntegration.Variables;

namespace TerraIntegration.ValueProperties.Numeric
{
    public class ToByte : ValueConversion
    {
        public override Type ConvertFrom => typeof(INumeric);
        public override Type ConvertTo => typeof(Values.Byte);

        public override bool AppliesTo(VariableValue value) => value is not Values.Byte;
        
        public override VariableValue GetProperty(VariableValue value, List<Error> errors)
        {
            long v = ((INumeric)value).NumericValue;
            return (VariableValue.GetInstance<Values.Byte>() as INumeric).GetFromNumeric(v, errors);
        }
    }
}
