using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerraIntegration.Basic;
using TerraIntegration.DataStructures;
using TerraIntegration.Values;

namespace TerraIntegration.Variables
{
    public abstract class ValueConversion : ValueProperty
    {
        internal static SpriteSheet ConvSheet = new("TerraIntegration/Assets/Types/conv", new(32, 32));

        public abstract Type[] ConvertFrom { get; }
        public abstract Type ConvertTo { get; }

        public virtual string DisplayName => "To {0}";

        public override Type VariableReturnType => ConvertTo;
        public override Type[] ValueTypes => ConvertFrom;
        public override string PropertyName 
        {
            get 
            {
                if (ConvertTo is null || !VariableValue.ByType.TryGetValue(ConvertTo, out VariableValue to)) 
                    return null;

                return $"conv.{to.Type}";
            }
        }
        public override string PropertyDisplay => string.Format(DisplayName, VariableValue.TypeToName(ConvertTo, true));
    }
}
