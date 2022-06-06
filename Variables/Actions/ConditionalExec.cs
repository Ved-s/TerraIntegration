﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerraIntegration.Basic;
using TerraIntegration.Basic.Actions;
using TerraIntegration.DataStructures;

namespace TerraIntegration.Variables.Actions
{
    public class ConditionalExec : ActionWithReference
    {
        public override VariableMatch Variables => VariableMatch.OfType<ActionVariable>();

        public override string TypeName => "condExec";
        public override string TypeDefaultDisplayName => "Conditional exec";

        public override bool VisibleInProgrammerVariables => true;

        public override void Execute(Point16 pos, Variable var, VariableValue refValue, ComponentSystem system, List<Error> errors)
        {
            if (var is ActionVariable action && refValue is Values.Boolean b && b.Value)
                action.Execute(pos, system, errors);
        }

        public override Type[] GetValidReferenceSlotTypes(Type leftSlotType) => new[] { typeof(Values.Boolean) };
    }
}