using System;
using System.Collections.Generic;
using TerraIntegration.Basic;
using TerraIntegration.Basic.References;
using TerraIntegration.DataStructures;

namespace TerraIntegration.ValueProperties.Tile
{
    public class TileSlope : ValueProperty<Values.Tile>
    {
        public override string PropertyName => "slope";
        public override string PropertyDisplay => "Slope type";

        public override string PropertyDescription => "Returns tile slope type";

        public override SpriteSheetPos SpriteSheetPos => new(TileSheet, 1, 3);

        public override ReturnType? VariableReturnType => typeof(Values.Byte);

        public override VariableValue GetProperty(ComponentSystem system, Values.Tile value, List<Error> errors)
            => new Values.Byte(value.Slope);
    }
}
