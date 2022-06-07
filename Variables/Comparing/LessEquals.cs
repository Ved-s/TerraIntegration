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
    public class LessEquals : DoubleReferenceVariableWithConst
    {
        public override string TypeName => "lessEquals";
        public override string TypeDefaultDisplayName => "Less or equals";
        public override string TypeDefaultDescription => "Returns True if left value is\nless or equals to right value";

        public override SpriteSheetPos SpriteSheetPos => new(ComparingSheet, 0, 1);

        public override Type[] LeftSlotValueTypes => new[] { typeof(Interfaces.IComparable) };

        public override Type VariableReturnType => typeof(Values.Boolean);

        public override Type[] GetValidRightSlotTypes(Type leftSlotType)
        {
            return new[] { leftSlotType };
        }

        public override VariableValue GetValue(ComponentSystem system, VariableValue left, VariableValue right, List<Error> errors)
        {
            Interfaces.IComparable comparable = left as Interfaces.IComparable;
            return new Values.Boolean(comparable.LessThan(right) || comparable.Equals(right));
        }
    }
}
