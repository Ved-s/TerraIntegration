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
    public interface ISubtractable : IValueInterface
    {
        public ReturnType[] ValidSubtractTypes { get; }
        public VariableValue Subtract(VariableValue value, List<Error> errors, TypeIdentity id);
    }
}
