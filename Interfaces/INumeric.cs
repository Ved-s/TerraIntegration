using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerraIntegration.Values;
using Terraria.ModLoader;

namespace TerraIntegration.Interfaces
{
    public interface INumeric : IAddable, IValueInterface
    {
        public long NumericValue { get; }
        public long NumericMax { get; }
        public long NumericMin { get; }

        Type[] IAddable.ValidAddTypes => new[] { typeof(INumeric), typeof(IDecimal) };

        public VariableValue GetFromNumeric(long value, List<Error> errors) 
        {
            if (!CheckNumericValue(value, errors)) return null;
            return FromNumericChecked(value, errors);
        }
        public bool CheckNumericValue(long value, List<Error> errors)
        {
            string type = (this as VariableValue)?.TypeDisplay;

            if (value > NumericMax)
            {
                errors?.Add(new(ErrorType.ValueTooBigForType, value, type));
                return false;
            }
            if (value < NumericMin)
            {
                errors?.Add(new(ErrorType.ValueTooSmallForType, value, type));
                return false;
            }
            return true;
        }

        protected VariableValue FromNumericChecked(long value, List<Error> errors);

        [NoJIT]
        VariableValue IAddable.Add(VariableValue value, List<Error> errors)
        {
            long val;

            if (value is INumeric numeric)
                val = numeric.NumericValue;
            else if (value is IDecimal @decimal)
                val = (long)@decimal.DecimalValue;
            else return null;

            long num = val + NumericValue;
            return GetFromNumeric(num, errors);
        }
    }
}
