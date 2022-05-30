using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerraIntegration.Components;
using TerraIntegration.Values;
using TerraIntegration.Variables;

namespace TerraIntegration.Basic
{
    public abstract class ValueProperty : ReferenceVariable
    {
        public static readonly Dictionary<Type, Dictionary<string, ValueProperty>> ByValueType = new();
        public static readonly List<ValueProperty> AllProperties = new();
        public static readonly List<ValueProperty> WaitingValue = new();

        public override string Type => $"{ValueTypeName}.{PropertyName}";

        public abstract Type ValueType { get; }
        public string ValueTypeName
        {
            get
            {
                if (ValueType is null) return null;

                if (VariableValue.ByType.TryGetValue(ValueType, out VariableValue val))
                    return val.Type;

                if (ValueType.IsInterface) 
                {
                    string name = ValueType.Name;
                    if (name.StartsWith('I')) return name[1..];
                    return name;
                }

                return ValueType.Name;
            }
        }

        public abstract string PropertyName { get; }
        public abstract string PropertyDisplay { get; }
        public virtual string PropertyDescription => "";

        public sealed override string TypeDescription => PropertyDescription;
        public sealed override string TypeDisplay => PropertyDisplay;

        public override Type ReferenceReturnType => ValueType;

        public abstract VariableValue GetProperty(ComponentSystem system, VariableValue value, List<Error> errors);

        public override VariableValue GetValue(ComponentSystem system, List<Error> errors)
        {
            VariableValue val = system.GetVariableValue(VariableId, errors);
            if (val is null) return null;

            if (!ValueType.IsAssignableFrom(val.GetType()))
            {
                errors.Add(new(ErrorType.ExpectedValue, VariableValue.TypeToName(ValueType, false)));
                return null;
            }

            return GetProperty(system, val, errors);
        }

        public static void ValueRegistered()
        {
            List<ValueProperty> success = new();

            foreach (ValueProperty prop in WaitingValue) 
            {
                if (prop.ValueType is null) 
                    continue;

                Register(prop);
                success.Add(prop);
            }

            foreach (ValueProperty pv in success)
                WaitingValue.Remove(pv);
        }

        public static void Register(ValueProperty property)
        {
            if (property.ValueType is null || property.PropertyName is null)
            {
                WaitingValue.Add(property);
                return;
            }

            if (!ByValueType.TryGetValue(property.ValueType, out var prop))
            {
                prop = new();
                ByValueType[property.ValueType] = prop;
            }
            prop[property.PropertyName] = property;
            ByTypeName[property.Type] = property;
        }

        public new static void Unregister() 
        {
            ByValueType.Clear();
            AllProperties.Clear();
        }

        public virtual bool AppliesTo(VariableValue value) => true;
    }

    public abstract class ValueProperty<TValue> : ValueProperty where TValue : VariableValue
    {
        public sealed override Type ValueType => typeof(TValue);

        public abstract VariableValue GetProperty(TValue value, List<Error> errors);

        public override VariableValue GetProperty(ComponentSystem system, VariableValue value, List<Error> errors)
        {
            return GetProperty(value as TValue, errors);
        }
    }
}
