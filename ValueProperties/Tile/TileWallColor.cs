using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerraIntegration.Basic;
using TerraIntegration.DataStructures;
using TerraIntegration.Templates;
using TerraIntegration.Values;

namespace TerraIntegration.ValueProperties.Tile
{
    public class TileWallColor : ValueProperty
    {
        public override Type[] ValueTypes => new[] { typeof(Values.Tile), typeof(Wall) };
        public override string PropertyName => "tileWallColor";
        public override string PropertyDisplay => "Color";

        public override string PropertyDescription => "Returns color id of this tile or wall";

        public override SpriteSheetPos SpriteSheetPos => new(TileSheet, 3, 3);

        public override ReturnType? VariableReturnType => typeof(Values.Byte);

        public override VariableValue GetProperty(ComponentSystem system, VariableValue value, List<Error> errors)
        {
            byte col;
            if (value is Values.Tile tile)
                col = tile.Color;
            else col = (value as Wall).Color;

            return new Values.Byte(col);
        }
    }
}
