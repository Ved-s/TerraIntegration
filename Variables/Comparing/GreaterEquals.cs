using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerraIntegration.Basic;
using TerraIntegration.DataStructures;
using TerraIntegration.Interfaces;
using TerraIntegration.Templates;
using TerraIntegration.Values;

namespace TerraIntegration.Variables.Comparing
{
    public class GreaterEquals : DoubleReferenceVariableWithConst
    {
        public override string TypeName => "greaterEquals";
        public override string TypeDefaultDisplayName => "Greater or equals";
        public override string TypeDefaultDescription => "Returns True if left value is\ngreater or eqal to right value";

        public override SpriteSheetPos SpriteSheetPos => new(ComparingSheet, 2, 0);

        public override ReturnType[] LeftSlotValueTypes => new ReturnType[] { typeof(Interfaces.IComparable) };

        public override ReturnType? VariableReturnType => typeof(Values.Boolean);

        public override ReturnType[] GetValidRightSlotTypes(ReturnType leftSlotType)
        {
            return new[] { leftSlotType };
        }

        public override VariableValue GetValue(ComponentSystem system, VariableValue left, VariableValue right, List<Error> errors)
        {
            Interfaces.IComparable comparable = left as Interfaces.IComparable;
            return new Values.Boolean(comparable.GreaterThan(right) || comparable.Equals(right));
        }
    }
}
