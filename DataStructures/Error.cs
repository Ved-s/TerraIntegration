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
        public ErrorType Type { get; set; }
        public string[] Args { get; set; }

        public Error(ErrorType type, params object[] args)
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

        public static Error ExpectedValue(Type valueType, Guid? variableId)
        {
            if (variableId is null)
                return new(ErrorType.ExpectedValue, VariableValue.TypeToName(valueType, false) ?? "null");
            return new(ErrorType.ExpectedValueWithId, VariableValue.TypeToName(valueType, false) ?? "null", ComponentWorld.Instance.Guids.GetShortGuid(variableId.Value));
        }
    }

    public enum ErrorType
    {
        // variable id
        VariableNotFound,

        // variable id
        RecursiveReference,

        // variable id
        MultipleVariablesSameID,

        // variable id
        ValueUnloaded,

        // variable id
        VariableUnloaded,

        // X, Y, expected type, found type
        WrongComponentAtPos,

        // X, Y, type
        NoComponentAtPos,

        // value type name
        ExpectedValue,

        // value type name, variable id
        ExpectedValueWithId,

        // value type names
        ExpectedValues,

        // value type names, variable id
        ExpectedValuesWithId,

        // value, type
        ValueTooBigForType,

        // value, type
        ValueTooSmallForType,
    }
}
