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
    public class IsEclipse : ComponentProperty<Components.WorldReader>
    {
        public override string PropertyName => "eclipse";
        public override string PropertyDisplay => "Is Eclipse";

        public override Type VariableReturnType => typeof(Values.Boolean);

        public override VariableValue GetProperty(Components.WorldReader component, Point16 pos, List<Error> errors)
        {
            return new Values.Boolean(Main.eclipse);
        }
    }
}
