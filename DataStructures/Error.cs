using System;
using System.Collections.Generic;
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

        public static Error ExpectedValue(Type valueType, Guid? variableId = null)
        {
            if (variableId is null)
                return new("ExpectedValue", VariableValue.TypeToName(valueType, false) ?? "null");
            return new("ExpectedValueWithId", VariableValue.TypeToName(valueType, false) ?? "null", ComponentWorld.Instance.Guids.GetShortGuid(variableId.Value));
        }
        public static Error ExpectedValues(IEnumerable<Type> valueTypes, Guid? variableId = null)
        {
            if (valueTypes is null) throw new ArgumentNullException(nameof(valueTypes));

            if (variableId is null)
                return new("ExpectedValues", string.Join(", ", valueTypes.Select(t => VariableValue.TypeToName(t, false))));
            return new("ExpectedValuesWithId", 
                string.Join(", ", valueTypes.Select(t => VariableValue.TypeToName(t, false))), 
                ComponentWorld.Instance.Guids.GetShortGuid(variableId.Value));
        }
        public static Error ExpectedVariable(Type variableType, Guid? variableId = null)
        {
            string varName = Variable.ByType.TryGetValue(variableType, out Variable var) ?
                var.TypeDisplayName : variableType.Name;

            if (variableId is null)
                return new("ExpectedVariable", varName);
            return new("ExpectedVariableWithId", varName, ComponentWorld.Instance.Guids.GetShortGuid(variableId.Value));
        }
        public static Error ExpectedVariables(string variableTypes, Guid? variableId = null)
        {
            if (variableId is null)
                return new("ExpectedVariables", variableTypes);
            return new("ExpectedVariablesWithId", variableTypes, ComponentWorld.Instance.Guids.GetShortGuid(variableId.Value));
        }

        public static Error ValueTooBigForType(object value, VariableValue valueType)
            => new("ValueTooBigForType", value, valueType?.TypeDisplayName ?? "null");
        
        public static Error ValueTooSmallForType(object value, VariableValue valueType)
            => new("ValueTooSmallForType", value, valueType?.TypeDisplayName ?? "null");
    }
}
