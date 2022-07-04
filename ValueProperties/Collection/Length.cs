using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerraIntegration.Basic;
using TerraIntegration.DataStructures;
using TerraIntegration.Interfaces.Value;
using TerraIntegration.Templates;
using TerraIntegration.Values;
using TerraIntegration.Variables;

namespace TerraIntegration.ValueProperties.Collection
{
    public class Length : ValueProperty
    {
        public override Type[] ValueTypes => new[] { typeof(ICollection) };
        public override string PropertyName => "len";
        public override string PropertyDisplay => "Length";
        public override string PropertyDescription => "Retutns length of the collection";

        public override ReturnType? VariableReturnType => typeof(Integer);

        public override SpriteSheetPos SpriteSheetPos => new(BasicSheet, 3, 1);

        public override bool AppliesTo(VariableValue value) => value is not Values.String;

        public override VariableValue GetProperty(ComponentSystem system, VariableValue value, List<Error> errors)
        {
            return new Integer(((ICollection)value).Enumerate(system, errors).Count());
        }
    }
}
