using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerraIntegration.Basic;
using TerraIntegration.DataStructures;

namespace TerraIntegration.ValueProperties.Tile
{
    public class TileLiquid : ValueProperty<Values.Tile>
    {
        public override string PropertyName => "liquid";
        public override string PropertyDisplay => "Liquid level";

        public override Type VariableReturnType => typeof(Values.Byte);

        public override VariableValue GetProperty(ComponentSystem system, Values.Tile value, List<Error> errors)
            => new Values.Byte(value.Liquid);
    }
}
