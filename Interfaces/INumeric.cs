using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerraIntegration.Values;

namespace TerraIntegration.Interfaces
{
    public interface INumeric
    {
        public long NumericValue { get; }
        public long NumericMax { get; }
        public long NumericMin { get; }

        public VariableValue GetFromNumeric(long value, List<Error> errors) 
        {
            string type = (this as VariableValue)?.TypeDisplay;

            if (value < NumericMin)
            {
                errors.Add(new(ErrorType.ValueTooSmallForType, value, type));
                return null;
            }
            if (value > NumericMax)
            {
                errors.Add(new(ErrorType.ValueTooBigForType, value, type));
                return null;
            }
            return FromNumericChecked(value, errors);
        }

        protected VariableValue FromNumericChecked(long value, List<Error> errors);
    }
}
