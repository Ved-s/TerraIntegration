using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerraIntegration.DataStructures;
using TerraIntegration.Interfaces;
using TerraIntegration.Values;
using TerraIntegration.Variables;

namespace TerraIntegration.ValueProperties.Collection
{
    public class First : ValueProperty
    {
        public override Type ValueType => typeof(ICollection);
        public override string PropertyName => "first";
        public override string PropertyDisplay => "First";

        public override SpriteSheetPos SpriteSheetPos => new(CollectionSheet, 0, 0);

        public override ReferenceVariable CreateVariable(Variable var)
        {
            First result = new();

            Type collectionReturn = var.VariableReturnType?.SubTypeA;
            if (collectionReturn is not null)
            {
                result.SetReturnTypeCache(ReturnValue.OfType(collectionReturn));
            }

            return result;
        }

        public override VariableValue GetProperty(VariableValue value, List<Error> errors)
        {
            ICollection collection = (ICollection)value;
            return collection.Enumerate().FirstOrDefault(new VariableValue());
        }
    }
}
