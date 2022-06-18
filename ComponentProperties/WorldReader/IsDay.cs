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
    public class IsDay : ComponentProperty<Components.WorldReader>
    {
        public override string PropertyName => "day";
        public override string PropertyDisplay => "Is Daytime";
        public override string TypeDefaultDescription => "Returns True during daytime";

        public override SpriteSheetPos SpriteSheetPos => new(1, 0);

        public override ReturnType? VariableReturnType => typeof(Values.Boolean);

        public override VariableValue GetProperty(Components.WorldReader component, Point16 pos, List<Error> errors)
        {
            return new Values.Boolean(Main.dayTime);
        }
    }
}
