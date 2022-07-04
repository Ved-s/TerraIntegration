using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerraIntegration.Basic;
using TerraIntegration.DataStructures;
using TerraIntegration.Interfaces.Value.Math;
using TerraIntegration.Values;
using Terraria.ModLoader;

namespace TerraIntegration.Interfaces.Value
{
    public interface INumeric : IMathOperable, IComparable, IValueInterface
    {
        public long NumericValue { get; }
        public long NumericMax { get; }
        public long NumericMin { get; }
        public byte BitWidth { get; }

        ReturnType[] IMathOperable.ValidMathValueTypes => new ReturnType[] { typeof(INumeric), typeof(IDecimal) };

        public VariableValue GetFromNumeric(long value, List<Error> errors)
        {
            if (!CheckNumericValue(value, errors)) return null;
            return FromNumericChecked(value, errors);
        }
        public bool CheckNumericValue(long value, List<Error> errors)
        {
            if (value > NumericMax)
            {
                errors?.Add(Errors.ValueTooBigForType(value, this as VariableValue));
                return false;
            }
            if (value < NumericMin)
            {
                errors?.Add(Errors.ValueTooSmallForType(value, this as VariableValue));
                return false;
            }
            return true;
        }

        protected VariableValue FromNumericChecked(long value, List<Error> errors);

        [NoJIT]
        VariableValue IAddable.Add(VariableValue value, List<Error> errors, TypeIdentity id)
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

        [NoJIT]
        VariableValue ISubtractable.Subtract(VariableValue value, List<Error> errors, TypeIdentity id)
        {
            long val;

            if (value is INumeric numeric)
                val = numeric.NumericValue;
            else if (value is IDecimal @decimal)
                val = (long)@decimal.DecimalValue;
            else return null;

            long num = NumericValue - val;
            return GetFromNumeric(num, errors);
        }

        [NoJIT]
        VariableValue IMultipliable.Multiply(VariableValue value, List<Error> errors, TypeIdentity id)
        {
            long val;

            if (value is INumeric numeric)
                val = numeric.NumericValue;
            else if (value is IDecimal @decimal)
                val = (long)@decimal.DecimalValue;
            else return null;

            long num = NumericValue * val;
            return GetFromNumeric(num, errors);
        }

        [NoJIT]
        VariableValue IDivisible.Divide(VariableValue value, List<Error> errors, TypeIdentity id)
        {
            long val;

            if (value is INumeric numeric)
                val = numeric.NumericValue;
            else if (value is IDecimal @decimal)
                val = (long)@decimal.DecimalValue;
            else return null;

            long num = NumericValue / val;
            return GetFromNumeric(num, errors);
        }

        [NoJIT]
        VariableValue IModulable.Modulo(VariableValue value, List<Error> errors, TypeIdentity id)
        {
            long val;

            if (value is INumeric numeric)
                val = numeric.NumericValue;
            else if (value is IDecimal @decimal)
                val = (long)@decimal.DecimalValue;
            else return null;

            long num = NumericValue % val;
            return GetFromNumeric(num, errors);
        }

        [NoJIT]
        bool IEquatable.Equals(VariableValue value)
        {
            return (value as INumeric).NumericValue == NumericValue;
        }

        [NoJIT]
        bool IComparable.GreaterThan(VariableValue value)
        {
            return NumericValue > (value as INumeric).NumericValue;
        }

        [NoJIT]
        bool IComparable.LessThan(VariableValue value)
        {
            return NumericValue < (value as INumeric).NumericValue;
        }
    }
}
