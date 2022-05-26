﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerraIntegration.Values;

namespace TerraIntegration.Interfaces.Math
{
    public interface IModulable : IValueInterface
    {
        public Type[] ValidModuloTypes { get; }
        public VariableValue Modulo(VariableValue value, List<Error> errors);
    }
}
