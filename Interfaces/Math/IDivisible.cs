using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerraIntegration.Basic;
using TerraIntegration.DataStructures;
using TerraIntegration.Values;

namespace TerraIntegration.Interfaces.Math
{
    public interface IDivisible : IValueInterface
    {
        public ReturnType[] ValidDivideTypes { get; }
        public VariableValue Divide(VariableValue value, List<Error> errors, TypeIdentity id);
    }
}
