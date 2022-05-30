using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerraIntegration.Basic;
using TerraIntegration.Values;
using TerraIntegration.Variables;
using Terraria;

namespace TerraIntegration.ComponentProperties.TileReader
{
    public class HasTile : ComponentProperty<Components.TileReader>
    {
        public override string PropertyName => "hasTile";
        public override string PropertyDisplay => "Has Tile";

        public override Type VariableReturnType => typeof(Values.Boolean);

        public override VariableValue GetProperty(Components.TileReader component, Point16 pos, List<Error> errors)
        {
            Terraria.Tile tile = component.GetTargetTile(pos);
            return new Values.Boolean(tile.HasTile);
        }
    }
}
