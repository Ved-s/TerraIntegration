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
    public interface IMultipliable : IValueInterface
    {
        public ReturnType[] ValidMultiplyTypes { get; }
        public VariableValue Multiply(VariableValue value, List<Error> errors, TypeIdentity id);
    }
}
