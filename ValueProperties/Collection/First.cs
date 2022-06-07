using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerraIntegration.Basic;
using TerraIntegration.DataStructures;
using TerraIntegration.Interfaces;
using TerraIntegration.Templates;
using TerraIntegration.Values;
using TerraIntegration.Variables;

namespace TerraIntegration.ValueProperties.Collection
{
    public class First : ValueProperty
    {
        public override Type[] ValueTypes => new[] { typeof(ICollection) };
        public override string PropertyName => "first";
        public override string PropertyDisplay => "First";
        public override string PropertyDescription => "Retutns first element of the collection";

        public override SpriteSheetPos SpriteSheetPos => new(CollectionSheet, 0, 0);

        public override ReferenceVariable CreateVariable(Variable var)
        {
            First result = new();

            Type collectionReturn = ICollection.TryGetCollectionType(var);
            if (collectionReturn is not null)
            {
                result.SetReturnTypeCache(collectionReturn);
            }

            return result;
        }

        public override VariableValue GetProperty(ComponentSystem system, VariableValue value, List<Error> errors)
        {
            ICollection collection = (ICollection)value;
            VariableValue val = collection.Enumerate(system, errors).FirstOrDefault();

            SetReturnTypeCache(val?.GetType());

            return val;
        }
    }
}
