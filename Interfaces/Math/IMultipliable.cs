using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerraIntegration.DataStructures;
using TerraIntegration.Values;

namespace TerraIntegration.Interfaces.Math
{
    public interface IMultipliable : IValueInterface
    {
        public ValueMatcher ValidMultiplyTypes { get; }
        public VariableValue Multiply(VariableValue value, List<Error> errors);
    }
}
