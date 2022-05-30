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
    public class GreaterThan : DoubleReferenceVariable
    {
        public override string Type => "greater";
        public override string TypeDisplay => ">";

        public override Type[] LeftSlotValueTypes => new[] { typeof(Interfaces.IComparable) };

        public override Type[] GetValidRightSlotTypes(Type leftSlotType)
        {
            return new[] { leftSlotType };
        }

        public override VariableValue GetValue(ComponentSystem system, VariableValue left, VariableValue right, List<Error> errors)
        {
            Interfaces.IComparable comparable = left as Interfaces.IComparable;
            return new Values.Boolean(comparable.GreaterThan(right));
        }
    }
}
