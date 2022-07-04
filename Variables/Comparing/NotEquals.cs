using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerraIntegration.Basic;
using TerraIntegration.DataStructures;
using TerraIntegration.Interfaces.Value;
using TerraIntegration.Templates;
using TerraIntegration.Values;

namespace TerraIntegration.Variables.Comparing
{
    public class NotEquals : DoubleReferenceVariableWithConst
    {
        public override string TypeName => "notEquals";
        public override string TypeDefaultDisplayName => "Not equals";
        public override string TypeDefaultDescription => "Returns True if values are not equal";

        public override SpriteSheetPos SpriteSheetPos => new(ComparingSheet, 1, 0);

        public override ReturnType[] LeftSlotValueTypes => new ReturnType[] { typeof(IEquatable) };

        public override ReturnType? VariableReturnType => typeof(Values.Boolean);

        public override ReturnType[] GetValidRightSlotTypes(ReturnType leftSlotType)
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
