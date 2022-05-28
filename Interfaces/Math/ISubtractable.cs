using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerraIntegration.DataStructures;
using TerraIntegration.Values;

namespace TerraIntegration.Interfaces.Math
{
    public interface ISubtractable : IValueInterface
    {
        public ValueMatcher ValidSubtractTypes { get; }
        public VariableValue Subtract(VariableValue value, List<Error> errors);
    }
}
