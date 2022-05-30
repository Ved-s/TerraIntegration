using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerraIntegration.Basic;
using TerraIntegration.Interfaces;
using TerraIntegration.Values;

namespace TerraIntegration.Variables.Comparing
{
    public class Equals : DoubleReferenceVariable
    {
        public override string Type => "equals";
        public override string TypeDisplay => "Equals";

        public override SpriteSheetPos SpriteSheetPos => new(BasicSheet, 2, 1);

        public override Type[] LeftSlotValueTypes => new[] { typeof(IEquatable) };

        public override Type[] GetValidRightSlotTypes(Type leftSlotType)
        {
            return new[] { leftSlotType };
        }

        public override VariableValue GetValue(ComponentSystem system, VariableValue left, VariableValue right, List<Error> errors)
        {
            bool result = (left as IEquatable).Equals(right);
            return new Values.Boolean(result);
        }
    }
}
