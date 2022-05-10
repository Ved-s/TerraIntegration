using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.Localization;
using Terraria.ModLoader;

namespace TerraIntegration
{
    public class Error
    {
        public ErrorType Type { get; set; }
        public object[] Args { get; set; }

        public Error(ErrorType type, params object[] args)
        {
            Type = type;
            Args = args;
        }

        public override bool Equals(object obj)
        {
            return obj is Error error &&
                   Type == error.Type &&
                   EqualityComparer<object[]>.Default.Equals(Args, error.Args);
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

            return $"{Type}: {string.Join(" ,", Args)}";
        }
    }

    public enum ErrorType
    {
        VariableNotFound,
        RecursiveReference,
        MultipleVariablesSameID,
        ValueUnloaded,
        VariableUnloaded,
        WrongComponentAtPos,
        NoComponentAtPos
    }
}
