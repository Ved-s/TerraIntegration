using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerraIntegration.Basic;
using TerraIntegration.DataStructures;
using TerraIntegration.Templates;

namespace TerraIntegration.ValueProperties.Tile
{
    public class TileLiquid : ValueProperty<Values.Objects.Tile>
    {
        public override string PropertyName => "liquid";
        public override string PropertyDisplay => "Liquid level";

        public override string PropertyDescription => "Returns liquid level in tile";

        public override SpriteSheetPos SpriteSheetPos => new(TileSheet, 2, 3);

        public override ReturnType? VariableReturnType => typeof(Values.Byte);

        public override VariableValue GetProperty(ComponentSystem system, Values.Objects.Tile value, List<Error> errors)
            => new Values.Byte(value.Liquid);
    }
}
