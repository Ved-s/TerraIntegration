using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerraIntegration.Components;
using TerraIntegration.Values;

namespace TerraIntegration.Variables.Display
{
    public class DisplayArea : PropertyVariable<Components.Display>
    {
        public override string TypeDisplay => "Display area";
        public override string ComponentProperty => "area";

        public override Type VariableReturnType => typeof(Integer);

        public override VariableValue GetProperty(Components.Display component, Point16 pos, HashSet<Error> errors)
        {
            DisplayData data = component.GetData(pos);
            return new Integer(data.DisplaySize.X * data.DisplaySize.Y);
        }
    }
}
