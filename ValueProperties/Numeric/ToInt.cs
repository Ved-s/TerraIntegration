using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerraIntegration.DataStructures;
using TerraIntegration.Interfaces;
using TerraIntegration.Values;
using TerraIntegration.Variables;

namespace TerraIntegration.ValueProperties.Numeric
{
    public class ToInt : ValueConversion
    {
        public override Type ConvertFrom => typeof(INumeric);
        public override Type ConvertTo => typeof(Integer);

        public override SpriteSheetPos SpriteSheetPos => new(ConvSheet, 3, 0);

        public override bool AppliesTo(VariableValue value) => value is not Integer;
        
        public override VariableValue GetProperty(VariableValue value, List<Error> errors)
        {
            long v = ((INumeric)value).NumericValue;
            return (VariableValue.GetInstance<Integer>() as INumeric).GetFromNumeric(v, errors);
        }
    }
}
