using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerraIntegration.Basic;
using TerraIntegration.Basic.References;
using TerraIntegration.DataStructures;
using TerraIntegration.Interfaces;
using TerraIntegration.Values;

namespace TerraIntegration.ValueProperties
{
    public class TypeProp : ValueProperty
    {
        public override System.Type[] ValueTypes => new[] { typeof(ITyped) };
        public override string PropertyName => "type";
        public override string PropertyDisplay => "Type";

        public override SpriteSheetPos SpriteSheetPos => new(BasicSheet, 1, 2);

        public override Type VariableReturnType => typeof(Integer);

        public override VariableValue GetProperty(ComponentSystem system, VariableValue value, List<Error> errors)
        {
            return new Integer(((ITyped)value).Type);
        }
    }
}
