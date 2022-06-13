using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerraIntegration.Basic;

namespace TerraIntegration.DataStructures
{
    public abstract class VariableMatch
    {
        public static VariableMatch OfType<TVariable>() where TVariable : Variable
        {
            return new TypeMatch(typeof(TVariable));
        }
        public static VariableMatch OfType(Type type)
        {
            return new TypeMatch(type);
        }
        public static VariableMatch OfType(string typeName)
        {
            return new VariableTypeNameMatch(typeName);
        }

        public static VariableMatch OfTypes(params Type[] types)
        {
            return new ManyTypeMatch(types);
        }

        public static VariableMatch OfReturnType<TValue>() where TValue : VariableValue
        {
            return new ReturnTypeMatch(typeof(TValue));
        }
        public static VariableMatch OfReturnType(Type type)
        {
            return new ReturnTypeMatch(type);
        }
        public static VariableMatch OfReturnTypes(params Type[] types)
        {
            return new ManyReturnTypeMatch(types);
        }

        public static VariableMatch Custom(string description,
                Func<Type, bool> type = null,
                Func<Variable, bool> variable = null,
                Func<VariableValue, bool> value = null)
        {
            return new CustomMatch(description, type, variable, value);
        }

        public abstract bool MatchType(Type type);
        public abstract bool MatchVariable(Variable var);
        public abstract bool MatchValue(VariableValue value);

        public IEnumerable<Variable> VariableMatches(IEnumerable<Variable> variables)
        {
            foreach (var variable in variables)
                if (MatchVariable(variable))
                    yield return variable;
        }

        public abstract string GetMatchDescription();

        public class TypeMatch : VariableMatch
        {
            public readonly Type Type;

            public TypeMatch(Type type)
            {
                Type = type ?? throw new ArgumentNullException(nameof(type));
            }

            public override string GetMatchDescription()
            {
                if (Variable.ByType.TryGetValue(Type, out Variable var))
                    return var.TypeDisplayName;

                return Type.Name;
            }

            public override bool MatchType(Type type)
            {
                if (type is null)
                    return false;

                return type.IsAssignableTo(Type);

            }
            public override bool MatchVariable(Variable var)
            {
                return var is not null && var.GetType().IsAssignableTo(Type);
            }
            public override bool MatchValue(VariableValue value)
            {
                return value is not null && value.GetType().IsAssignableTo(Type);
            }
        }

        public class VariableTypeNameMatch : VariableMatch
        {
            public readonly string TypeName;

            public VariableTypeNameMatch(string typeName)
            {
                TypeName = typeName ?? throw new ArgumentNullException(nameof(typeName));
            }

            public override string GetMatchDescription()
            {
                if (Variable.ByTypeName.TryGetValue(TypeName, out Variable var))
                    return var.TypeDisplayName;

                return TypeName;
            }

            public override bool MatchType(Type type)
            {
                if (type is null) return false;

                return Variable.ByType.TryGetValue(type, out Variable var) && var.TypeName == TypeName;
            }
            public override bool MatchVariable(Variable var)
            {
                return var?.TypeName == TypeName;
            }

            public override bool MatchValue(VariableValue value) => false;
        }

        public class ManyTypeMatch : VariableMatch
        {
            public readonly Type[] Types;

            public ManyTypeMatch(Type[] types)
            {
                Types = types ?? throw new ArgumentNullException(nameof(types));
            }

            public override string GetMatchDescription()
            {
                return string.Join(", ", Types.Select(t => Variable.ByType.TryGetValue(t, out Variable var) ?  var.TypeDisplayName : t.Name));
            }

            public override bool MatchType(Type type)
            {
                if (type is null) return false;

                return Types.Any(t => t.IsAssignableFrom(type));
            }

            public override bool MatchVariable(Variable var)
            { 
                if (var is null)
                    return false;

                Type varType = var.GetType();

                return Types.Any(t => t.IsAssignableFrom(varType));
            }

            public override bool MatchValue(VariableValue value)
            {
                if (value is null)
                    return false;

                Type valueType = value.GetType();

                return Types.Any(t => t.IsAssignableFrom(valueType));
            }
        }

        public class ReturnTypeMatch : VariableMatch
        {
            public readonly Type ReturnType;

            public ReturnTypeMatch(Type returnType)
            {
                ReturnType = returnType ?? throw new ArgumentNullException(nameof(returnType));
            }

            public override string GetMatchDescription()
            {
                return VariableValue.TypeToName(ReturnType, true);
            }

            public override bool MatchType(Type type)
            {
                return type is not null && type.IsAssignableTo(ReturnType);
            }

            public override bool MatchVariable(Variable var)
            {
                return var.VariableReturnType is not null && var.VariableReturnType.IsAssignableTo(ReturnType);
            }

            public override bool MatchValue(VariableValue value)
            {
                if (value is null)
                    return false;

                Type valueType = value.GetType();
                return valueType.IsAssignableTo(ReturnType);
            }
        }

        public class ManyReturnTypeMatch : VariableMatch
        {
            public readonly Type[] ReturnTypes;

            public ManyReturnTypeMatch(Type[] returnTypes)
            {
                ReturnTypes = returnTypes ?? throw new ArgumentNullException(nameof(returnTypes));
            }

            public override string GetMatchDescription()
            {
                return string.Join(", ", ReturnTypes.Select(t => VariableValue.TypeToName(t, true)));
            }

            public override bool MatchType(Type type)
            {
                return type is not null && ReturnTypes.Any(t => type.IsAssignableTo(t));
            }

            public override bool MatchVariable(Variable var)
            {
                return var.VariableReturnType is not null && ReturnTypes.Any(t => var.VariableReturnType.IsAssignableTo(t));
            }

            public override bool MatchValue(VariableValue value)
            {
                if (value is null)
                    return false;

                Type valueType = value.GetType();
                return ReturnTypes.Any(t => valueType.IsAssignableTo(t));
            }
        }

        public class CustomMatch : VariableMatch
        {
            public readonly Func<Variable, bool> Variable;
            public readonly Func<VariableValue, bool> Value;
            public readonly Func<Type, bool> Type;

            public readonly string MatchDescription;

            public CustomMatch(string matchDescription, 
                Func<Type, bool> type = null, 
                Func<Variable, bool> variable = null,
                Func<VariableValue, bool> value = null)
            {
                Type = type;
                Value = value;
                Variable = variable;

                MatchDescription = matchDescription;
            }

            public override string GetMatchDescription() => MatchDescription;

            public override bool MatchType(Type type)
            {
                if (Type is null)
                {
                    if (type.IsSubclassOf(typeof(Variable)))
                        return Variable is not null && Basic.Variable.ByType.TryGetValue(type, out Variable var) && Variable(var);

                    if (type.IsSubclassOf(typeof(VariableValue)))
                        return Value is not null && Basic.VariableValue.ByType.TryGetValue(type, out VariableValue value) && Value(value);

                    return false;
                }
                return Type(type);
            }
            public override bool MatchVariable(Variable var)
            {
                if (Variable is null)
                {
                    if (Type is not null)
                        return Type(var.GetType());
                    return false;
                }

                return Variable(var);
            }
            public override bool MatchValue(VariableValue value)
            {
                if (Value is null)
                {
                    if (Type is not null)
                        return Type(value.GetType());
                    return false;
                }

                return Value(value);
            }

        }
    }
}
