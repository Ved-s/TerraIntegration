using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerraIntegration.Interfaces;
using TerraIntegration.Values;

namespace TerraIntegration.DataStructures
{
    public struct ReturnValue
    {
        public static ReturnValue Default = OfType<VariableValue>();

        public Type ValueType { get; set; }

        public Type SubTypeA { get; set; }
        public Type SubTypeB { get; set; }

        public static ReturnValue OfType(Type type) => new() { ValueType = type };
        public static ReturnValue OfType<TValue>() where TValue : VariableValue => new() { ValueType = typeof(TValue) };

        public ReturnValue WithSubA<TValue>() where TValue : VariableValue
            => this with { SubTypeA = typeof(TValue) };
        public ReturnValue WithSubB<TValue>() where TValue : VariableValue
            => this with { SubTypeB = typeof(TValue) };

        public VariableValue GetInstance()
            => VariableValue.ByType.TryGetValue(ValueType, out var value) ? value : null;

        public TValue GetInstance<TValue>() where TValue : VariableValue
            => VariableValue.ByType.TryGetValue(ValueType, out var value) ? value as TValue : null;

        public TInterface GetInstanceInterface<TInterface>() where TInterface : class, IValueInterface
            => GetInstance() as TInterface;

        public bool CheckType(Type type) => ValueType == type;
        public bool CheckType<TValue>() where TValue : VariableValue
            => ValueType == typeof(TValue);

        public bool CheckBaseType(Type type) => type is not null && ValueType.IsAssignableTo(type);
        public bool CheckBaseType<TValue>() where TValue : VariableValue
            => ValueType == typeof(TValue);
        public bool CheckInterface<TInterface>() where TInterface : class, IValueInterface
            => ValueType.IsAssignableTo(typeof(TInterface));

        public string DisplayName(bool colored) 
        {
            if (ValueType is null) return null;

            StringBuilder sb = new();
            sb.Append(VariableValue.TypeToName(ValueType, colored));

            if (SubTypeA is not null)
            {
                sb.Append(" of ");
                sb.Append(VariableValue.TypeToName(SubTypeA, colored));
                if (SubTypeB is not null)
                {
                    sb.Append(", ");
                    sb.Append(VariableValue.TypeToName(SubTypeB, colored));
                }
            }
            return sb.ToString();
        }

        public override string ToString()
        {
            Type[] types = new[] { ValueType, SubTypeA, SubTypeB };
            return string.Join(";", types.Select(t => t?.FullName ?? ""));
        }
        public static ReturnValue Parse(string str)
        {
            ReturnValue ret = new();

            Type[] types = str.Split(';').Select(s => s.IsNullEmptyOrWhitespace() ? null : Type.GetType(s)).ToArray();
            ret.ValueType = types.Length <= 0 ? null : types[0];
            ret.SubTypeA = types.Length <= 1 ? null : types[1];
            ret.SubTypeB = types.Length <= 2 ? null : types[2];

            return ret;
        }
    }
}
