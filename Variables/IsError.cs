using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerraIntegration.Basic;
using TerraIntegration.DataStructures;
using TerraIntegration.Templates;
using TerraIntegration.Values;

namespace TerraIntegration.Variables
{
    public class IsError : ReferenceVariable
    {
        public override string TypeName => "iserror";
        public override string TypeDefaultDisplayName => "Is Error";
        public override string TypeDefaultDescription => "Returns True if variable it's\nbind to returns an error";

        public override SpriteSheetPos SpriteSheetPos => new(BasicSheet, 1, 1);

        public override ReturnType? VariableReturnType => typeof(Values.Boolean);

        List<Error> ErrorTest = new();

        public override VariableValue GetValue(ComponentSystem system, List<Error> errors)
        {
            ErrorTest.Clear();
            system.GetVariableValue(VariableId, ErrorTest);
            return new Values.Boolean(ErrorTest.Count > 0);
        }
        public override VariableValue GetValue(VariableValue value, ComponentSystem system, List<Error> errors)
        {
            throw new InvalidOperationException();
        }
    }
}
