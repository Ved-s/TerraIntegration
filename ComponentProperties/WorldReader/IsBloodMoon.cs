using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerraIntegration.Basic;
using TerraIntegration.DataStructures;
using Terraria;

namespace TerraIntegration.ComponentProperties.WorldReader
{
    public class IsBloodMoon : ComponentProperty<Components.WorldReader>
    {
        public override string PropertyName => "bloodMoon";
        public override string PropertyDisplay => "Is Blood Moon";
        public override string TypeDefaultDescription => "Returns True during Blood Moon";

        public override SpriteSheetPos SpriteSheetPos => new(0, 0);

        public override ReturnType? VariableReturnType => typeof(Values.Boolean);

        public override VariableValue GetProperty(Components.WorldReader component, Point16 pos, List<Error> errors)
        {
            return new Values.Boolean(Main.bloodMoon);
        }
    }
}
