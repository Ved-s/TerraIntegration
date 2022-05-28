using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerraIntegration.DataStructures;
using TerraIntegration.Values;
using TerraIntegration.Variables;

namespace TerraIntegration.ComponentProperties.TileReader
{
    public class TileData : ComponentProperty<Components.TileReader>
    {
        public override string PropertyName => "tiledata";
        public override string PropertyDisplay => "Tile";

        public override ReturnValue? VariableReturnType => ReturnValue.OfType<Tile>();

        public override VariableValue GetProperty(Components.TileReader component, Point16 pos, List<Error> errors)
        {
            Terraria.Tile tile = component.GetTargetTile(pos);
            if (!tile.HasTile)
                return new Tile();

            return new Tile(tile);
        }
    }
}
