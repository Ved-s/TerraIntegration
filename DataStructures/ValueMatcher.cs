using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerraIntegration.Variables;

namespace TerraIntegration.DataStructures
{
    public struct ValueMatcher
    {
        public static ValueMatcher MatchNone => new();

        public bool MatchesNone => MatchTypes is null && CustomMatch is null && !DefaultMatch;

        public Type[] MatchTypes;
        public Func<ReturnValue, bool> CustomMatch;
        public bool DefaultMatch = false;

        public bool Match(ReturnValue? returnValue)
        {
            if (returnValue is null) 
                return DefaultMatch;

            if (MatchTypes is null && CustomMatch is null) return DefaultMatch;

            if (!MatchTypes.Any(t => returnValue.Value.CheckBaseType(t)))
                return false;

            if (CustomMatch is not null) 
                return CustomMatch(returnValue.Value);

            return true;
        }

        public static ValueMatcher OfType(Type type) => new() { MatchTypes = new[] { type } };
        public static ValueMatcher OfTypes(Type[] types) => new() { MatchTypes = types };
        public static ValueMatcher OfType<T>() => new() { MatchTypes = new[] { typeof(T) } };
        public static ValueMatcher OfTypes<T1, T2>() => new() { MatchTypes = new[] { typeof(T1), typeof(T2) } };
        public static ValueMatcher OfTypes<T1, T2, T3>() => new() { MatchTypes = new[] { typeof(T1), typeof(T2), typeof(T3) } };

        public static ValueMatcher Custom(Func<ReturnValue, bool> match) => new() { CustomMatch = match };
    }
}
