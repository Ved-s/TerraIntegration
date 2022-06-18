using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerraIntegration.Basic;
using TerraIntegration.DataStructures;
using TerraIntegration.Values;

namespace TerraIntegration.Templates
{
    public abstract class ValueConversion : ValueProperty
    {
        internal static SpriteSheet ConvSheet = new("TerraIntegration/Assets/Types/conv", new(32, 32));

        public abstract Type[] ConvertFrom { get; }
        public abstract Type ConvertTo { get; }

        public override string PropertyDisplay => "To {0}";
        public override string PropertyDescription => "Converts value to {0}";

        public override object[] DisplayNameFormatters => new[] { VariableValue.TypeToName(ConvertTo, true) };
        public override object[] DescriptionFormatters => new[] { VariableValue.TypeToName(ConvertTo, true) };

        public override ReturnType? VariableReturnType => ConvertTo;
        public override Type[] ValueTypes => ConvertFrom;
        public override string PropertyName
        {
            get
            {
                if (ConvertTo is null || !VariableValue.ByType.TryGetValue(ConvertTo, out VariableValue to))
                    return null;

                return $"conv.{to.TypeName}";
            }
        }
    }
}
