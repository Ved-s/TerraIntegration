using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TerraIntegration.Basic;

namespace TerraIntegration.DataStructures
{
    public struct ReturnType
    {
        static Regex NonEscapedSemicolon = new(@"(?<!\\);", RegexOptions.Compiled);

        public static Dictionary<Type, Func<Type, IEnumerable<ReturnType>>> CustomTypeSelectors { get; } = new()
        {
            [typeof(Interfaces.Value.ICollection)] = (type) => Util.Enum(Interfaces.Value.ICollection.TryGetCollectionType(type))
        };

        public Type Type { get; } = typeof(VariableValue);
        public ReturnType[] SubType { get; }

        public ReturnType(Type type, params ReturnType[] subType)
        {
            Type = type;
            SubType = subType;
        }

        public ReturnType(Type type) : this()
        {
            Type = type;
            SubType = null;
        }

        public bool Match(Type returnType)
        {
            return returnType is not null && returnType.IsAssignableTo(Type);
        }

        public bool Match(ReturnType? returnType)
        {
            if (returnType?.Type == typeof(VariableValue))
                return true;

            return returnType.HasValue 
                && returnType.Value.Type is not null
                && returnType.Value.Type.IsAssignableTo(Type) 
                && (SubType is null 
                    || returnType.Value.SubType is null 
                    || MatchSubtypes(returnType.Value.SubType));
        }

        bool MatchSubtypes(ReturnType[] sub)
        {
            return SubType.Zip(sub).All((t) => t.First.Match(t.Second));
        }

        public IEnumerable<ReturnType> GetAllTypes()
        {
            IEnumerable<ReturnType> ienum = Util.Enum(this);
            foreach (var kvp in CustomTypeSelectors)
            {
                if (Type.IsAssignableTo(kvp.Key))
                    ienum = ienum.Concat(kvp.Value(Type));
            }
            return ienum;
        }

        public string ToStringName(bool colored)
        {
            if (VariableValue.ByType.TryGetValue(Type, out VariableValue value))
            {
                string format = value.FormatReturnType(this, colored);
                if (format is not null)
                    return format;
            }

            string type = VariableValue.TypeToName(Type, colored);
            if (type is null)
                return null;

            if (SubType?.Length is null or 0)
                return type;

            return $"{type}{(SubType?.Length is not null and > 0 ? " of " + string.Join(", ", SubType.Select(t => t.ToStringName(colored))) : null)}";
        }
        public string ToTypeString()
        {
            string type = VariableValue.ByType.TryGetValue(Type, out VariableValue val) ? "V" + val.TypeName : "T" + Type.FullName;

            if (SubType?.Length is not null and > 0)
            {
                type += ";" + string.Join(';', SubType.Select(sub => sub.ToTypeString().Replace(";", "\\;")));
            }
            return type;
        }

        public static ReturnType? FromTypeString(string str)
        {
            if (str.IsNullEmptyOrWhitespace())
                return null;

            string[] split = str.Split(';', count: 2);

            if (split.Length == 0)
                return null;

            bool istype = split[0].StartsWith('T');
            split[0] = split[0][1..];

            Type type = null;

            if (istype)
                type = Type.GetType(split[0]);

            else if (VariableValue.ByTypeName.TryGetValue(split[0], out VariableValue val))
                type = val.GetType();

            ReturnType[] sub = null;

            if (split.Length > 1)
            {
                string[] subsplit = NonEscapedSemicolon.Split(split[1]);
                sub = subsplit.Select(s => FromTypeString(s.Replace("\\;", ";")) ?? typeof(VariableValue)).ToArray();
            }

            return new(type, sub);
        }

        public override bool Equals(object obj)
        {
            return obj is ReturnType type &&
                   Util.ObjectsNullEqual(Type, type.Type) &&
                   Util.ObjectsNullEqual(SubType, type.SubType, (a, b) => a.Length == b.Length && a.SequenceEqual(b));
        }

        public override int GetHashCode()
        {
            int hash = Type?.GetHashCode() ?? 13888956;

            if (SubType is not null)
            {
                hash = HashCode.Combine(hash, SubType.Length);

                foreach (ReturnType t in SubType)
                    hash = HashCode.Combine(hash, t.GetHashCode());
            }

            return hash;
        }

        public static implicit operator ReturnType(Type type) => new(type);

        public static bool operator ==(ReturnType? a, ReturnType? b)
        {
            if (a.HasValue != b.HasValue)
                return false;
            if (!a.HasValue)
                return true;

            if (a.Value.Type != b.Value.Type)
                return false;

            if (a.Value.SubType is null != b.Value.SubType is null)
                return false;

            if (a.Value.SubType is null)
                return true;

            return a.Value.SubType.SequenceEqual(b.Value.SubType);
        }
        public static bool operator !=(ReturnType? a, ReturnType? b) => !(a == b);
    }
}
