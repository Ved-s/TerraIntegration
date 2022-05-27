using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerraIntegration.Interfaces;
using TerraIntegration.Values;
using TerraIntegration.Variables;

namespace TerraIntegration.ValueProperties.Collection
{
    public class Length : ValueProperty
    {
        public override Type ValueType => typeof(ICollection);
        public override string PropertyName => "len";
        public override string PropertyDisplay => "Length";

        public override Type VariableReturnType => typeof(Integer);

        public override SpriteSheetPos SpriteSheetPos => new(BasicSheet, 3, 1);

        public override bool AppliesTo(VariableValue value) => value is not Values.String;

        public override VariableValue GetProperty(VariableValue value, List<Error> errors)
        {
            return new Integer(((ICollection)value).Enumerate().Count());
        }
    }
}
