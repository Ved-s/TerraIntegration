using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerraIntegration.Basic;
using TerraIntegration.DataStructures;
using TerraIntegration.Values;
using TerraIntegration.Variables;

namespace TerraIntegration.ComponentProperties.TileReader
{
    public class WallData : ComponentProperty<Components.TileReader>
    {
        public override string PropertyName => "walldata";
        public override string PropertyDisplay => "Wall";
        public override string TypeDefaultDescription => "Returns data of a wall in front of Tile Reader";

        public override SpriteSheetPos SpriteSheetPos => new(2, 0);

        public override ReturnType? VariableReturnType => typeof(Wall);

        public override VariableValue GetProperty(Components.TileReader component, Point16 pos, List<Error> errors)
        {
            return new Wall(component.GetTargetTile(pos));
        }
    }
}
