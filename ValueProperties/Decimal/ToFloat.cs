using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerraIntegration.Basic;
using TerraIntegration.DataStructures;
using TerraIntegration.Interfaces;
using TerraIntegration.Values;
using TerraIntegration.Variables;

namespace TerraIntegration.ValueProperties.Decimal
{
    public class ToFloat : ValueConversion
    {
        public override Type ConvertFrom => typeof(IDecimal);
        public override Type ConvertTo => typeof(Float);

        public override SpriteSheetPos SpriteSheetPos => new(ConvSheet, 1, 1);

        public override bool AppliesTo(VariableValue value) => value is not Float;

        public override VariableValue GetProperty(ComponentSystem system, VariableValue value, List<Error> errors)
        {
            double v = ((IDecimal)value).DecimalValue;
            return (VariableValue.GetInstance<Float>() as IDecimal).GetFromDecimal(v, errors);
        }
    }
}
