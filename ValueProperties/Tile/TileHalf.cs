using System;
using System.Collections.Generic;
using TerraIntegration.Basic;
using TerraIntegration.DataStructures;

namespace TerraIntegration.ValueProperties.Tile
{
    public class TileHalf : ValueProperty<Values.Tile>
    {
        public override string PropertyName => "half";
        public override string PropertyDisplay => "Is half block";

        public override SpriteSheetPos SpriteSheetPos => new(TileSheet, 0, 3);

        public override Type VariableReturnType => typeof(Values.Boolean);

        public override VariableValue GetProperty(ComponentSystem system, Values.Tile value, List<Error> errors)
            => new Values.Boolean(value.HalfBlock);
    }
}
