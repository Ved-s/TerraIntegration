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
    public class RainTime : ComponentProperty<Components.WorldReader>
    {
        public override string PropertyName => "rainTime";
        public override string PropertyDisplay => "Rain time";
        public override string TypeDefaultDescription => "Returns amount of ticks until rain ends";

        public override SpriteSheetPos SpriteSheetPos => new(0, 1);

        public override ReturnType? VariableReturnType => typeof(Values.Integer);

        public override VariableValue GetProperty(Components.WorldReader component, Point16 pos, List<Error> errors)
        {
            return new Values.Integer((int)Main.rainTime);
        }
    }
}
