using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerraIntegration.Interfaces;
using TerraIntegration.Values;
using TerraIntegration.Variables;

namespace TerraIntegration.ValueProperties
{
    public class ToString : ValueProperty
    {
        public override Type ValueType => typeof(IToString);
        public override string PropertyName => "toStr";
        public override string PropertyDisplay => "To String";

        public override Type VariableReturnType => typeof(Values.String);

        public override SpriteSheetPos SpriteSheetPos => new(StringSheet, 0, 0);

        public override bool AppliesTo(VariableValue value) => value is not Values.String;

        public override VariableValue GetProperty(VariableValue value, List<Error> errors)
        {
            return new Values.String(((IToString) value).ToString());
        }
    }
}
