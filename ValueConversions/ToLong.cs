using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerraIntegration.Basic;
using TerraIntegration.DataStructures;
using TerraIntegration.Interfaces.Value;
using TerraIntegration.Templates;
using TerraIntegration.Values;
using TerraIntegration.Variables;

namespace TerraIntegration.ValueConversions
{
    public class ToLong : ValueConversion
    {
        public override Type[] ConvertFrom => new[] { typeof(INumeric), typeof(IDecimal) };
        public override Type ConvertTo => typeof(Long);

        public override SpriteSheetPos SpriteSheetPos => new(ConvSheet, 2, 1);

        public override bool AppliesTo(VariableValue value) => value is not Long;
        
        public override VariableValue GetProperty(ComponentSystem system, VariableValue value, List<Error> errors)
        {
            long v;
            if (value is IDecimal @decimal)
                v = (long)@decimal.DecimalValue;
            else v = ((INumeric)value).NumericValue;

            return (VariableValue.GetInstance<Long>() as INumeric).GetFromNumeric(v, errors);
        }
    }
}
