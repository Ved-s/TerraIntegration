using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerraIntegration.Basic;
using TerraIntegration.DataStructures;
using TerraIntegration.Values;

namespace TerraIntegration.Interfaces.Value.Math
{
    public interface IModulable : IValueInterface
    {
        public ReturnType[] ValidModuloTypes { get; }
        public VariableValue Modulo(VariableValue value, List<Error> errors, TypeIdentity id);
    }
}
