using System;
using System.Collections.Generic;
using TerraIntegration.Basic;
using TerraIntegration.Basic.References;
using TerraIntegration.DataStructures;
using TerraIntegration.Interfaces;
using TerraIntegration.Values;
using TerraIntegration.Variables;

namespace TerraIntegration.ValueConversions
{
    public class ToDouble : ValueConversion
    {
        public override Type[] ConvertFrom => new[] { typeof(IDecimal), typeof(INumeric) };
        public override Type ConvertTo => typeof(Values.Double);

        public override SpriteSheetPos SpriteSheetPos => new(ConvSheet, 0, 1);

        public override bool AppliesTo(VariableValue value) => value is not Values.Double;

        public override VariableValue GetProperty(ComponentSystem system, VariableValue value, List<Error> errors)
        {
            double v;
            if (value is INumeric numeric)
                v = numeric.NumericValue;
            else v = ((IDecimal)value).DecimalValue;

            return (VariableValue.GetInstance<Values.Double>() as IDecimal).GetFromDecimal(v, errors);
        }
    }
}
