using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerraIntegration.Basic;
using TerraIntegration.DataStructures;
using TerraIntegration.Interfaces;
using TerraIntegration.Values;

namespace TerraIntegration.Variables.Comparing
{
    public class NotEquals : DoubleReferenceVariableWithConst
    {
        public override string TypeName => "notEquals";
        public override string TypeDefaultDisplayName => "Not equals";

        public override SpriteSheetPos SpriteSheetPos => new(ComparingSheet, 1, 0);

        public override Type[] LeftSlotValueTypes => new[] { typeof(IEquatable) };

        public override Type VariableReturnType => typeof(Values.Boolean);

        public override Type[] GetValidRightSlotTypes(Type leftSlotType)
        {
            return new[] { leftSlotType };
        }

        public override VariableValue GetValue(ComponentSystem system, VariableValue left, VariableValue right, List<Error> errors)
        {
            bool result = !(left as IEquatable).Equals(right);
            return new Values.Boolean(result);
        }
    }
}
