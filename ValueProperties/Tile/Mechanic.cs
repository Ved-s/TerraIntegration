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
    //public class TileActuated : ValueProperty<Values.Objects.Tile>
    //{
    //    public override string PropertyName => "actuated";
    //    public override string PropertyDisplay => "Is actuated";
    //    public override string PropertyDescription => "Returns whether this tile is actuated";
    //
    //    public override ReturnType? VariableReturnType => typeof(Values.Boolean);
    //
    //    public override SpriteSheetPos SpriteSheetPos => new(TileSheet, 2, 1);
    //
    //    public override VariableValue GetProperty(ComponentSystem system, Values.Objects.Tile value, List<Error> errors)
    //        => new Values.Boolean(value.Actuated);
    //}

    public class TileActuator : ValueProperty<Values.Objects.Tile>
    {
        public override string PropertyName => "actuator";
        public override string PropertyDisplay => "Has actuator";
        public override string PropertyDescription => "Returns whether this tile has an actuator on it";

        public override ReturnType? VariableReturnType => typeof(Values.Boolean);

        public override SpriteSheetPos SpriteSheetPos => new(TileSheet, 3, 1);

        public override VariableValue GetProperty(ComponentSystem system, Values.Objects.Tile value, List<Error> errors)
            => new Values.Boolean(value.Actuator);
    }

    public class TileWireRed : ValueProperty<Values.Objects.Tile>
    {
        public override string PropertyName => "wireRed";
        public override string PropertyDisplay => "Has red wire";
        public override string PropertyDescription => "Returns whether this tile has red wire on it";

        public override ReturnType? VariableReturnType => typeof(Values.Boolean);

        public override SpriteSheetPos SpriteSheetPos => new(TileSheet, 0, 2);

        public override VariableValue GetProperty(ComponentSystem system, Values.Objects.Tile value, List<Error> errors)
            => new Values.Boolean(value.RedWire);
    }

    public class TileWireGreen : ValueProperty<Values.Objects.Tile>
    {
        public override string PropertyName => "wireGreen";
        public override string PropertyDisplay => "Has green wire";

        public override string PropertyDescription => "Returns whether this tile has green wire on it";

        public override ReturnType? VariableReturnType => typeof(Values.Boolean);

        public override SpriteSheetPos SpriteSheetPos => new(TileSheet, 1, 2);

        public override VariableValue GetProperty(ComponentSystem system, Values.Objects.Tile value, List<Error> errors)
            => new Values.Boolean(value.GreenWire);
    }

    public class TileWireBlue : ValueProperty<Values.Objects.Tile>
    {
        public override string PropertyName => "wireBlue";
        public override string PropertyDisplay => "Has blue wire";

        public override string PropertyDescription => "Returns whether this tile has blue wire on it";

        public override ReturnType? VariableReturnType => typeof(Values.Boolean);

        public override SpriteSheetPos SpriteSheetPos => new(TileSheet, 2, 2);

        public override VariableValue GetProperty(ComponentSystem system, Values.Objects.Tile value, List<Error> errors)
            => new Values.Boolean(value.BlueWire);
    }

    public class TileWireYellow : ValueProperty<Values.Objects.Tile>
    {
        public override string PropertyName => "wireYellow";
        public override string PropertyDisplay => "Has yellow wire";

        public override string PropertyDescription => "Returns whether this tile has yellow wire on it";

        public override ReturnType? VariableReturnType => typeof(Values.Boolean);

        public override SpriteSheetPos SpriteSheetPos => new(TileSheet, 3, 2);

        public override VariableValue GetProperty(ComponentSystem system, Values.Objects.Tile value, List<Error> errors)
            => new Values.Boolean(value.YellowWire);
    }
}
