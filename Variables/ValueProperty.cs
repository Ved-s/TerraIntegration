using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerraIntegration.Components;
using TerraIntegration.UI;
using TerraIntegration.Values;
using TerraIntegration.Variables;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader.IO;

namespace TerraIntegration.Variables
{
    public abstract class ValueProperty : Variable, IOwnProgrammerInterface
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

        public Guid VariableId { get; set; }

        public override string TypeDescription
        {
            get
            {
                string propd = PropertyDescription;

                string result = "";
                if (VariableId != default)
                {
                    result = $"[c/aaaa00:Variable ID:] {World.Guids.GetShortGuid(VariableId)}";
                }

                if (!propd.IsNullEmptyOrWhitespace())
                {
                    if (result.IsNullEmptyOrWhitespace()) result = propd;
                    else result += "\n" + propd;
                }

                return result;
            }
        }

        public virtual string PropertyDescription => "";

        public UIPanel Interface { get; set; }
        public UIVariableSlot InterfaceSlot { get; set; }

        public abstract VariableValue GetProperty(VariableValue value, List<Error> errors);

        public virtual ValueProperty CreateVariable(Variable var)
        {
            ValueProperty prop = (ValueProperty)Activator.CreateInstance(GetType());
            prop.VariableId = var.Id;

            return prop;
        }

        public override VariableValue GetValue(ComponentSystem system, List<Error> errors)
        {
            VariableValue val = system.GetVariableValue(VariableId, errors);
            if (val is null) return null;

            return GetProperty(val, errors);
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
            if (property.ValueType is null)
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

        public static void Unregister() 
        {
            ByValueType.Clear();
            AllProperties.Clear();
        }

        protected override void SaveCustomData(BinaryWriter writer)
        {
            writer.Write(VariableId.ToByteArray());
        }
        protected override Variable LoadCustomData(BinaryReader reader)
        {
            ValueProperty prop = (ValueProperty)Activator.CreateInstance(GetType());

            prop.VariableId = new(reader.ReadBytes(16));
            return prop;
        }

        protected override object SaveCustomTag()
        {
            return new TagCompound()
            {
                ["var"] = VariableId.ToByteArray(),
            };
        }
        protected override Variable LoadCustomTag(object data)
        {
            ValueProperty prop = (ValueProperty)Activator.CreateInstance(GetType());

            if (data is TagCompound tag)
            {
                if (tag.ContainsKey("var"))
                    prop.VariableId = new(tag.GetByteArray("var"));
            }

            return prop;
        }

        public void SetupInterface()
        {
            UIPanel p = new();
            Interface = p;

            InterfaceSlot = new()
            {
                DisplayOnly = true,
                Top = new(-21, .5f),
                Left = new(-21, .5f),
                VariableValidator = (var) => ValueType?.IsAssignableFrom(var.VariableReturnType) ?? false
            };
            p.Append(InterfaceSlot);
        }

        public void WriteVariable(Items.Variable var)
        {
            if (InterfaceSlot.Var is not null)
            {
                var.Var = CreateVariable(InterfaceSlot.Var.Var);
            }
        }
    }

    public abstract class ValueProperty<TValue> : ValueProperty where TValue : VariableValue
    {
        public sealed override Type ValueType => typeof(TValue);

        public abstract VariableValue GetProperty(TValue value, List<Error> errors);

        public override VariableValue GetProperty(VariableValue value, List<Error> errors)
        {
            return GetProperty(value as TValue, errors);
        }
    }
}
