using System;
using System.Collections.Generic;
using TerraIntegration.Values;
using Terraria.ModLoader;

namespace TerraIntegration.Interfaces
{
    public interface IDecimal : IAddable
    {
        public double DecimalValue { get; }
        public double DecimalMax { get; }
        public double DecimalMin { get; }

        Type[] IAddable.ValidAddTypes => new[] { typeof(INumeric), typeof(IDecimal) };

        public VariableValue GetFromDecimal(double value, List<Error> errors)
        {
            if (!CheckDecimalValue(value, errors)) return null;
            return FromDecimalChecked(value, errors);
        }
        public bool CheckDecimalValue(double value, List<Error> errors)
        {
            string type = (this as VariableValue)?.TypeDisplay;

            if (value > DecimalMax)
            {
                errors?.Add(new(ErrorType.ValueTooBigForType, value, type));
                return false;
            }
            if (value < DecimalMin)
            {
                errors?.Add(new(ErrorType.ValueTooSmallForType, value, type));
                return false;
            }
            return true;
        }

        protected VariableValue FromDecimalChecked(double value, List<Error> errors);

        [NoJIT]
        VariableValue IAddable.Add(VariableValue value, List<Error> errors)
        {
            double val;

            if (value is INumeric numeric)
                val = numeric.NumericValue;
            else if (value is IDecimal @decimal)
                val = (long)@decimal.DecimalValue;
            else return null;

            double num = val + DecimalValue;
            return GetFromDecimal(num, errors);
        }
    }
}
