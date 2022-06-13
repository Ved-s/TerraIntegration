using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerraIntegration.Basic;
using Terraria.Localization;
using Terraria.ModLoader;

namespace TerraIntegration.DataStructures
{
    public class Error
    {
        public string Type { get; set; }
        public string[] Args { get; set; }

        public Error(string type, params object[] args)
        {
            Type = type;
            Args = args.Select(o => o.ToString()).ToArray();
        }

        public Error(string type, params string[] args)
        {
            Type = type;
            Args = args;
        }

        public override bool Equals(object obj)
        {
            return obj is Error error &&
                   Type == error.Type &&
                   Args.SequenceEqual(error.Args);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Type, Args);
        }

        public override string ToString()
        {
            string langKeyPath = $"Mods.{ModContent.GetInstance<TerraIntegration>().Name}.Errors.{Type}";

            if (LanguageManager.Instance.Exists(langKeyPath))
            {
                return LanguageManager.Instance.GetTextValue(langKeyPath, Args);
            }

            return $"{Type}: {string.Join(", ", Args)}";
        }

        public void NetSend(BinaryWriter writer)
        {
            writer.Write(Type);
            writer.Write(Args?.Length ?? 0);

            if (Args?.Length is not null and > 0)
                foreach (string arg in Args)
                    writer.Write(arg);
        }
        public static Error NetReceive(BinaryReader reader)
        {
            string type = reader.ReadString();
            int count = reader.ReadInt32();
            string[] args = new string[count];

            for (int i = 0; i < count; i++)
            {
                args[i] = reader.ReadString();
            }

            return new Error(type, args);
        }
    }

    public static class Errors 
    {
        public static Error VariableNotFound(Guid varId) => new("VariableNotFound", ComponentWorld.Instance.Guids.GetShortGuid(varId));
        public static Error RecursiveReference(Guid varId) => new("RecursiveReference", ComponentWorld.Instance.Guids.GetShortGuid(varId));
        public static Error MultipleVariablesSameID(Guid varId) => new("MultipleVariablesSameID", ComponentWorld.Instance.Guids.GetShortGuid(varId));

        public static Error ValueUnloaded(Guid varId) => new("ValueUnloaded", ComponentWorld.Instance.Guids.GetShortGuid(varId));
        public static Error VariableUnloaded(Guid varId) => new("VariableUnloaded", ComponentWorld.Instance.Guids.GetShortGuid(varId));

        public static Error WrongComponentAtPos(Point16 pos, string expectedType, string foundType)
            => new("WrongComponentAtPos", pos.X, pos.Y, expectedType, foundType);
        public static Error NoComponentAtPos(Point16 pos, string type)
            => new("NoComponentAtPos", pos.X, pos.Y, type);

        public static Error ExpectedValue(Type valueType, TypeIdentity id)
            => new("ExpectedValue", id.ToString(), VariableValue.TypeToName(valueType, false) ?? "null");

        public static Error ExpectedValues(IEnumerable<Type> valueTypes, TypeIdentity id)
        {
            if (valueTypes is null) throw new ArgumentNullException(nameof(valueTypes));

            return new("ExpectedValues", id.ToString(),
                string.Join(", ", valueTypes.Select(t => VariableValue.TypeToName(t, false))));
        }
        public static Error ExpectedValues(string values, TypeIdentity id)
        {
            if (values is null) throw new ArgumentNullException(nameof(values));

            return new("ExpectedValues", id.ToString(), values);
        }

        public static Error ExpectedVariable(Type variableType, TypeIdentity id)
        {
            string varName = Variable.ByType.TryGetValue(variableType, out Variable var) ?
                var.TypeDisplayName : variableType.Name;

            return new("ExpectedVariable", id.ToString(), varName);
        }
        public static Error ExpectedVariables(string variableTypes, TypeIdentity id)
            => new("ExpectedVariables", id.ToString(), variableTypes);

        public static Error ValueTooBigForType(object value, VariableValue valueType)
            => new("ValueTooBigForType", value, valueType?.TypeDisplayName ?? "null");
        
        public static Error ValueTooSmallForType(object value, VariableValue valueType)
            => new("ValueTooSmallForType", value, valueType?.TypeDisplayName ?? "null");
    }
}
