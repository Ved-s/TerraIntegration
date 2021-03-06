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
    public class WindTime : ComponentProperty<Components.WorldReader>
    {
        public override string PropertyName => "windTime";
        public override string PropertyDisplay => "Wind time";
        public override string TypeDefaultDescription => "Returns amount of ticks until wind changes";

        public override SpriteSheetPos SpriteSheetPos => new(2, 1);

        public override ReturnType? VariableReturnType => typeof(Values.Integer);

        public override VariableValue GetProperty(Components.WorldReader component, Point16 pos, List<Error> errors)
        {
            return new Values.Integer(Main.windCounter);
        }
    }
}
