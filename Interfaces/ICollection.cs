using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerraIntegration.Values;

namespace TerraIntegration.Interfaces
{
    public interface ICollection : IValueInterface
    {
        public IEnumerable<VariableValue> Enumerate();

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
    }

    public interface ICollection<T> : ICollection
    {
    }
}
