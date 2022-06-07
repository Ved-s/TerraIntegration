using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerraIntegration.Basic;
using TerraIntegration.DataStructures;
using TerraIntegration.Templates;

namespace TerraIntegration.Variables.Comparing
{
    public class ValueChanged : ReferenceVariable
    {
        public override string TypeName => "changed";
        public override string TypeDefaultDisplayName => "Value Changed";
        public override string TypeDefaultDescription => "Returns True once value changes";

        public override SpriteSheetPos SpriteSheetPos => new(BasicSheet, 0, 3);

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
