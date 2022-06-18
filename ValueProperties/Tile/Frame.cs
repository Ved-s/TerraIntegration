using System;
using System.Collections.Generic;
using TerraIntegration.Basic;
using TerraIntegration.DataStructures;
using TerraIntegration.Templates;
using TerraIntegration.Values;

namespace TerraIntegration.ValueProperties.Tile
{
    public class FrameX : ValueProperty
    {
        public override Type[] ValueTypes => new[] { typeof(Values.Tile), typeof(Wall) };
        public override string PropertyName => "frameX";
        public override string PropertyDisplay => "Frame X";
        public override string PropertyDescription => "Gets X component of tile frame position";

        public override SpriteSheetPos SpriteSheetPos => new(TileSheet, 3, 0);

        public override ReturnType? VariableReturnType => typeof(Integer);

        public override VariableValue GetProperty(ComponentSystem system, VariableValue value, List<Error> errors)
        {
            int frx;
            if (value is Values.Tile tile)
                frx = tile.TileFrameX;
            else frx = (value as Wall).WallFrameX;
            return new Integer(frx);
        }
    }

    public class FrameY : ValueProperty
    {
        public override Type[] ValueTypes => new[] { typeof(Values.Tile), typeof(Wall) };
        public override string PropertyName => "frameY";
        public override string PropertyDisplay => "Frame Y";
        public override string PropertyDescription => "Gets Y component of tile frame position";

        public override SpriteSheetPos SpriteSheetPos => new(TileSheet, 0, 1);

        public override ReturnType? VariableReturnType => typeof(Integer);

        public override VariableValue GetProperty(ComponentSystem system, VariableValue value, List<Error> errors)
        {
            int fry;
            if (value is Values.Tile tile)
                fry = tile.TileFrameY;
            else fry = (value as Wall).WallFrameY;
            return new Integer(fry);
        }
    }

    public class FrameNumber : ValueProperty
    {
        public override Type[] ValueTypes => new[] { typeof(Values.Tile), typeof(Wall) };
        public override string PropertyName => "frameNum";
        public override string PropertyDisplay => "Frame Number";
        public override string PropertyDescription => "Gets tile frame number";

        public override SpriteSheetPos SpriteSheetPos => new(TileSheet, 1, 1);

        public override ReturnType? VariableReturnType => typeof(Integer);

        public override VariableValue GetProperty(ComponentSystem system, VariableValue value, List<Error> errors)
        {
            int frn;
            if (value is Values.Tile tile)
                frn = tile.TileFrameNumber;
            else frn = (value as Wall).WallFrameNumber;
            return new Integer(frn);
        }
    }
}
