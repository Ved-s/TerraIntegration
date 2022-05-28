using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerraIntegration.DataStructures;
using TerraIntegration.Values;

namespace TerraIntegration.Interfaces.Math
{
    public interface IModulable : IValueInterface
    {
        public ValueMatcher ValidModuloTypes { get; }
        public VariableValue Modulo(VariableValue value, List<Error> errors);
    }
}
