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
    public interface IAddable : IValueInterface
    {
        public ReturnType[] ValidAddTypes { get; }
        public VariableValue Add(VariableValue value, List<Error> errors, TypeIdentity id);
    }
}
