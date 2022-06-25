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
using Terraria.ModLoader;

namespace TerraIntegration.ValueProperties.Collection
{
    public class AsCollection : ValueProperty
    {
        public override Type[] ValueTypes => new[] { typeof(ICollection) };
        public override string PropertyName => "asCollection";
        public override string PropertyDisplay => "As Collection";

        public override ReferenceVariable CreateVariable(Variable var)
        {
            return new AsCollection
            {
                VariableReturnType = new(typeof(ICollection), ICollection.TryGetCollectionType(var) ?? typeof(VariableValue))
            };
        }

        public override VariableValue GetProperty(ComponentSystem system, VariableValue value, List<Error> errors)
        {
            return value;
        }
    }
}
