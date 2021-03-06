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
    public class WindSpeed : ComponentProperty<Components.WorldReader>
    {
        public override string PropertyName => "windSpeed";
        public override string PropertyDisplay => "Wind speed";
        public override string TypeDefaultDescription => "Returns wind speed as float value.\nRanges from -0.8 to 0.8";

        public override SpriteSheetPos SpriteSheetPos => new(1, 1);

        public override ReturnType? VariableReturnType => typeof(Values.Float);

        public override VariableValue GetProperty(Components.WorldReader component, Point16 pos, List<Error> errors)
        {
            return new Values.Float(Main.windSpeedCurrent);
        }
    }
}
