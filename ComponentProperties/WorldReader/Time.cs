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
    public class Time : ComponentProperty<Components.WorldReader>
    {
        public override string PropertyName => "time";
        public override string PropertyDisplay => "Time";

        public override Type VariableReturnType => typeof(Values.Integer);

        public override VariableValue GetProperty(Components.WorldReader component, Point16 pos, List<Error> errors)
        {
            return new Values.Integer((int)Main.time);
        }
    }
}
