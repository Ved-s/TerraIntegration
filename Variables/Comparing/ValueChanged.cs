using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerraIntegration.Basic;
using TerraIntegration.DataStructures;

namespace TerraIntegration.Variables.Comparing
{
    public class ValueChanged : ReferenceVariable
    {
        public override string Type => "changed";
        public override string TypeDisplay => "Value Changed";

        public override Type VariableReturnType => typeof(Values.Boolean);

        public VariableValue OldValue;

        public override VariableValue GetValue(VariableValue value, ComponentSystem system, List<Error> errors)
        {
            bool changed = false;
            if (OldValue is not null && !OldValue.Equals(value))
                changed = true;
            
            OldValue = value;
            return new Values.Boolean(changed);
        }
    }
}
