using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerraIntegration.Basic;
using TerraIntegration.DataStructures;
using TerraIntegration.Values;
using TerraIntegration.Variables;

namespace TerraIntegration.Interfaces
{
    public interface ICollection : IValueInterface
    {
        public IEnumerable<VariableValue> Enumerate(ComponentSystem system, List<Error> errors);

        public ReturnType CollectionType { get; }

        public static ReturnType TryGetCollectionType(Type collection) 
        {
            if (collection is null) return typeof(VariableValue);

            Type interf;
            if (collection.IsGenericType && collection.GetGenericTypeDefinition() == typeof(ICollection<>))
                interf = collection;

            else interf = collection.FindInterfaces(
                (t, _) => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(ICollection<>), null)
                .FirstOrDefault();

            return interf?.GetGenericArguments().FirstOrDefault() ?? typeof(VariableValue);
        }

        public static ReturnType? TryGetCollectionType(Variable var)
        {
            if (var is null)
                return null;

            if (var is Constant @const)
            {
                if (@const.Value is ICollection collection)
                {
                    ReturnType t = collection.CollectionType;
                    if (t.Type != typeof(VariableValue)) 
                        return t;
                }
            }

            if (var.VariableReturnType?.Type == typeof(ICollection))
            {
                return var.VariableReturnType.Value.SubType.FirstOrDefault();
            }

            return TryGetCollectionType(var.VariableReturnType?.Type);
        }

        public static Type OfType(Type valueType)
        {
            return typeof(ICollection<>).MakeGenericType(valueType);
        }
    }

    public interface ICollection<T> : ICollection
    {
        ReturnType ICollection.CollectionType => typeof(T);
    }
}
