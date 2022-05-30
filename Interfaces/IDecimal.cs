using System;
using System.Collections.Generic;
using TerraIntegration.Basic;
using TerraIntegration.DataStructures;
using TerraIntegration.Interfaces.Math;
using TerraIntegration.Values;
using Terraria.ModLoader;

namespace TerraIntegration.Interfaces
{
    public interface IDecimal : IMathOperable, IComparable, IValueInterface
    {
        public double DecimalValue { get; }
        public double DecimalMax { get; }
        public double DecimalMin { get; }

        Type[] IMathOperable.ValidMathValueTypes => new[] { typeof(INumeric), typeof(IDecimal) };

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

        [NoJIT]
        VariableValue ISubtractable.Subtract(VariableValue value, List<Error> errors)
        {
            double val;

            if (value is INumeric numeric)
                val = numeric.NumericValue;
            else if (value is IDecimal @decimal)
                val = (long)@decimal.DecimalValue;
            else return null;

            double num = DecimalValue - val;
            return GetFromDecimal(num, errors);
        }

        [NoJIT]
        VariableValue IMultipliable.Multiply(VariableValue value, List<Error> errors)
        {
            double val;

            if (value is INumeric numeric)
                val = numeric.NumericValue;
            else if (value is IDecimal @decimal)
                val = (long)@decimal.DecimalValue;
            else return null;

            double num = DecimalValue * val;
            return GetFromDecimal(num, errors);
        }

        [NoJIT]
        VariableValue IDivisible.Divide(VariableValue value, List<Error> errors)
        {
            double val;

            if (value is INumeric numeric)
                val = numeric.NumericValue;
            else if (value is IDecimal @decimal)
                val = (long)@decimal.DecimalValue;
            else return null;

            double num = DecimalValue / val;
            return GetFromDecimal(num, errors);
        }

        [NoJIT]
        VariableValue IModulable.Modulo(VariableValue value, List<Error> errors)
        {
            double val;

            if (value is INumeric numeric)
                val = numeric.NumericValue;
            else if (value is IDecimal @decimal)
                val = (long)@decimal.DecimalValue;
            else return null;

            double num = DecimalValue % val;
            return GetFromDecimal(num, errors);
        }

        [NoJIT]
        bool IEquatable.Equals(VariableValue value)
        {
            return (value as IDecimal).DecimalValue == DecimalValue;
        }

        [NoJIT]
        bool IComparable.GreaterThan(VariableValue value)
        {
            return DecimalValue > (value as IDecimal).DecimalValue;
        }

        [NoJIT]
        bool IComparable.LessThan(VariableValue value)
        {
            return DecimalValue < (value as IDecimal).DecimalValue;
        }

    }
}
