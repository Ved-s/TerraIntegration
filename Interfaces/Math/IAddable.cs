using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerraIntegration.DataStructures;
using TerraIntegration.Values;

namespace TerraIntegration.Interfaces.Math
{
    public interface IAddable : IValueInterface
    {
        public ValueMatcher ValidAddTypes { get; }
        public VariableValue Add(VariableValue value, List<Error> errors);
    }
}
