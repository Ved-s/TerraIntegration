using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerraIntegration.Basic;
using TerraIntegration.Components;
using TerraIntegration.DataStructures;
using TerraIntegration.Templates;
using TerraIntegration.Values;
using TerraIntegration.Variables;

namespace TerraIntegration.Templates
{
    public abstract class ValueProperty : ReferenceVariable
    {
        public static readonly Dictionary<Type, Dictionary<string, ValueProperty>> ByValueType = new();
        public static readonly List<ValueProperty> AllProperties = new();
        public static readonly List<ValueProperty> WaitingValue = new();

        public sealed override string TypeName => $"{(ValueTypeName is null ? "" : ValueTypeName + ".")}{PropertyName}";

        public abstract Type[] ValueTypes { get; }

        public string ValueTypeName
        {
            get
            {
                if (ValueTypes is null || ValueTypes.Length != 1) return null;

                if (VariableValue.ByType.TryGetValue(ValueTypes[0], out VariableValue val))
                    return val.TypeName;

                if (ValueTypes[0].IsInterface)
                {
                    string name = ValueTypes[0].Name;
                    if (name.StartsWith('I')) return name[1..];
                    return name;
                }

                return ValueTypes[0].Name;
            }
        }

        public abstract string PropertyName { get; }
        public abstract string PropertyDisplay { get; }
        public virtual string PropertyDescription => null;

        public sealed override string TypeDefaultDescription => PropertyDescription;
        public sealed override string TypeDefaultDisplayName => PropertyDisplay;

        protected override VariableMatch InitReferenceMatch => VariableMatch.OfReturnTypes(ValueTypes);
        protected override VariableMatch InitRelated => VariableMatch.OfReturnTypes(ValueTypes);
        public override bool VisibleInProgrammerVariables => false;

        public abstract VariableValue GetProperty(ComponentSystem system, VariableValue value, List<Error> errors);

        public override VariableValue GetValue(VariableValue value, ComponentSystem system, List<Error> errors)
        {
            if (!ValueTypes.Any(t => t.IsAssignableFrom(value.GetType())))
            {
                errors.Add(Errors.ExpectedValues(ValueTypes, TypeIdentity));
                return null;
            }

            return GetProperty(system, value, errors);
        }

        public static void ValueRegistered()
        {
            List<ValueProperty> success = new();

            foreach (ValueProperty prop in WaitingValue)
            {
                if (prop.ValueTypes is null)
                    continue;

                Register(prop);
                success.Add(prop);
            }

            foreach (ValueProperty pv in success)
                WaitingValue.Remove(pv);
        }

        public static void Register(ValueProperty property)
        {
            if (property.ValueTypes is null || property.PropertyName is null)
            {
                WaitingValue.Add(property);
                return;
            }

            foreach (Type valueType in property.ValueTypes)
            {

                if (!ByValueType.TryGetValue(valueType, out var prop))
                {
                    prop = new();
                    ByValueType[valueType] = prop;
                }
                prop[property.PropertyName] = property;
            }
            ByTypeName[property.TypeName] = property;
            Register(property, true);
        }

        public new static void Unregister()
        {
            ByValueType.Clear();
            AllProperties.Clear();
        }

        public virtual bool AppliesTo(VariableValue value) => true;
    }
}

namespace TerraIntegration.Basic.References
{
    public abstract class ValueProperty<TValue> : ValueProperty where TValue : VariableValue
    {
        public sealed override Type[] ValueTypes => new[] { typeof(TValue) };

        public abstract VariableValue GetProperty(ComponentSystem system, TValue value, List<Error> errors);

        public override VariableValue GetProperty(ComponentSystem system, VariableValue value, List<Error> errors)
        {
            return GetProperty(system, value as TValue, errors);
        }
    }
}
