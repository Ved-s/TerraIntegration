using Microsoft.Xna.Framework;
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

namespace TerraIntegration.ValueConversions
{
    public class ToString : ValueConversion
    {
        public override Type[] ConvertFrom => new[] { typeof(IToString) };
        public override Type ConvertTo => typeof(Values.String);

        public override SpriteSheetPos SpriteSheetPos => new(ConvSheet, 0, 0);

        public override bool AppliesTo(VariableValue value) => value is not Values.String;

        public override VariableValue GetProperty(ComponentSystem system, VariableValue value, List<Error> errors)
        {
            return new Values.String(((IToString) value).ToString());
        }
    }
}
