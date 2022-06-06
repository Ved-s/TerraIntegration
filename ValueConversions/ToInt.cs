using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerraIntegration.Basic;
using TerraIntegration.Basic.References;
using TerraIntegration.DataStructures;
using TerraIntegration.Interfaces;
using TerraIntegration.Values;
using TerraIntegration.Variables;

namespace TerraIntegration.ValueConversions
{
    public class ToInt : ValueConversion
    {
        public override Type[] ConvertFrom => new[] { typeof(INumeric), typeof(IDecimal) };
        public override Type ConvertTo => typeof(Integer);

        public override SpriteSheetPos SpriteSheetPos => new(ConvSheet, 3, 0);

        public override bool AppliesTo(VariableValue value) => value is not Integer;
        
        public override VariableValue GetProperty(ComponentSystem system, VariableValue value, List<Error> errors)
        {
            long v;
            if (value is IDecimal @decimal)
                v = (long)@decimal.DecimalValue;
            else v = ((INumeric)value).NumericValue;

            return (VariableValue.GetInstance<Integer>() as INumeric).GetFromNumeric(v, errors);
        }
    }
}
