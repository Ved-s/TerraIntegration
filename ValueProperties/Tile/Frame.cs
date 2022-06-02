using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerraIntegration.Basic;
using TerraIntegration.DataStructures;
using TerraIntegration.Values;

namespace TerraIntegration.ValueProperties.Tile
{
    public class FrameX : ValueProperty
    {
        public override Type[] ValueTypes => new[] { typeof(Values.Tile), typeof(Wall) };
        public override string PropertyName => "frameX";
        public override string PropertyDisplay => "Frame X";

        public override Type VariableReturnType => typeof(Integer);

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

        public override Type VariableReturnType => typeof(Integer);

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

        public override Type VariableReturnType => typeof(Integer);

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
