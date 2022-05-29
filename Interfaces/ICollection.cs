using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerraIntegration.Values;
using TerraIntegration.Variables;

namespace TerraIntegration.Interfaces
{
    public interface ICollection : IValueInterface
    {
        public IEnumerable<VariableValue> Enumerate(ComponentSystem system, List<Error> errors);

        public Type CollectionType { get; }

        public static Type TryGetCollectionType(Type collection) 
        {
            if (collection is null) return null;

            Type interf;
            if (collection.IsGenericType && collection.GetGenericTypeDefinition() == typeof(ICollection<>))
                interf = collection;

            else interf = collection.FindInterfaces(
                (t, _) => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(ICollection<>), null)
                .FirstOrDefault();

            return interf?.GetGenericArguments().FirstOrDefault();
        }

        public static Type TryGetCollectionType(Variable var)
        {
            if (var is Constant @const)
            {
                if (@const.Value is ICollection collection)
                {
                    Type t = collection.CollectionType;
                    if (t != typeof(VariableValue)) 
                        return t;
                }
            }
            return TryGetCollectionType(var.VariableReturnType);
        }
    }

    public interface ICollection<T> : ICollection
    {
        Type ICollection.CollectionType => typeof(T);
    }
}
