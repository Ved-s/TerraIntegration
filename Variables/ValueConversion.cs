using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerraIntegration.Values;

namespace TerraIntegration.Variables
{
    public abstract class ValueConversion : ValueProperty
    {
        public abstract Type ConvertFrom { get; }
        public abstract Type ConvertTo { get; }

        public virtual string DisplayName => "To {0}";

        public override Type VariableReturnType => ConvertTo;
        public override Type ValueType => ConvertFrom;
        public override string PropertyName 
        {
            get 
            {
                if (ConvertTo is null || !VariableValue.ByType.TryGetValue(ConvertTo, out VariableValue to)) 
                    return null;

                return $"conv.{to.Type}";
            }
        }
        public override string PropertyDisplay => string.Format(DisplayName, VariableValue.TypeToColorTagName(ConvertTo));
    }
}
