﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerraIntegration.Values;

namespace TerraIntegration.Variables
{
    public class IsError : ReferenceVariable
    {
        public override string Type => "iserror";
        public override string TypeDisplay => "Is Error";

        public override Type VariableReturnType => typeof(Values.Boolean);

        List<Error> ErrorTest = new();

        public override VariableValue GetValue(ComponentSystem system, List<Error> errors)
        {
            ErrorTest.Clear();
            system.GetVariableValue(VariableId, ErrorTest);
            return new Values.Boolean(ErrorTest.Count > 0);
        }
    }
}