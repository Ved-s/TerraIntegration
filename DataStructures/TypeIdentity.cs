using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerraIntegration.Basic;

namespace TerraIntegration.DataStructures
{
    public struct TypeIdentity
    {
        object Target;
        Point16 Pos;

        private TypeIdentity(object targ, Point16 pos)
        {
            Target = targ;
            Pos = pos;
        }

        public static TypeIdentity Variable(Variable var) => new(var, default);

        public static TypeIdentity Component(PositionedComponent component) => new(component.Component, component.Pos);

        public override string ToString() 
        {
            if (Target is Variable var)
                return $"Variable {var.Name ?? ComponentWorld.Instance.Guids.GetShortGuid(var.Id)}";
            else if (Target is Basic.Component com)
                return $"Component {com.TypeName} at {Pos.X} {Pos.Y}";
            return "";
        }
    }
}
