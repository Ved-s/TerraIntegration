using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerraIntegration.Basic;
using TerraIntegration.Values;

namespace TerraIntegration.Interfaces.Value
{
    public interface IEquatable : IValueInterface
    {
        public bool Equals(VariableValue value);
    }
}
