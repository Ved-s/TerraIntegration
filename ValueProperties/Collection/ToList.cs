using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerraIntegration.Basic;
using TerraIntegration.Interfaces;
using TerraIntegration.Values;
using TerraIntegration.Variables;
using Terraria.ModLoader;

namespace TerraIntegration.ValueProperties.Collection
{
    [Autoload(false)]
    public class ToCollection : ValueProperty
    {
        public override Type ValueType => typeof(ICollection);
        public override string PropertyName => "toCollection";
        public override string PropertyDisplay => "To Collection";

        public override ReferenceVariable CreateVariable(Variable var)
        {
            ToCollection result = new ToCollection();

            Type collectionType = ICollection.TryGetCollectionType(var);

            if (collectionType is not null)
            {
                result.SetReturnTypeCache(typeof(Interfaces.ICollection<>).MakeGenericType(collectionType));
            }

            return result;
        }

        public override VariableValue GetProperty(ComponentSystem system, VariableValue value, List<Error> errors)
        {
            return value;
        }
    }
}
