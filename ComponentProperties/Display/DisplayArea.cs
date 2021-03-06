using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerraIntegration.Basic;
using TerraIntegration.Components;
using TerraIntegration.DataStructures;
using TerraIntegration.Values;
using TerraIntegration.Variables;

namespace TerraIntegration.ComponentProperties.Display
{
    public class DisplayArea : ComponentProperty<Components.Display>
    {
        public override string PropertyDisplay => "Display area";
        public override string PropertyName => "area";
        public override string TypeDefaultDescription => "Returns display area";

        public override SpriteSheetPos SpriteSheetPos => new(1, 0);

        public override ReturnType? VariableReturnType => typeof(Integer);

        public override VariableValue GetProperty(Components.Display component, Point16 pos, List<Error> errors)
        {
            DisplayData data = component.GetData(pos);
            return new Integer(data.Size.X * data.Size.Y);
        }
    }
}
