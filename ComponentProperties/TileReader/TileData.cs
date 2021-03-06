using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerraIntegration.Basic;
using TerraIntegration.DataStructures;
using TerraIntegration.Values.Objects;
using TerraIntegration.Variables;

namespace TerraIntegration.ComponentProperties.TileReader
{
    public class TileData : ComponentProperty<Components.TileReader>
    {
        public override string PropertyName => "tiledata";
        public override string PropertyDisplay => "Tile";
        public override string TypeDefaultDescription => "Returns data of a tile in front of Tile Reader";

        public override SpriteSheetPos SpriteSheetPos => new(1, 0);

        public override ReturnType? VariableReturnType => typeof(Tile);

        public override VariableValue GetProperty(Components.TileReader component, Point16 pos, List<Error> errors)
        {
            Terraria.Tile tile = component.GetTargetTile(pos);
            return new Tile(tile);
        }
    }
}
