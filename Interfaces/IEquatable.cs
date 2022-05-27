using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerraIntegration.Values;

namespace TerraIntegration.Interfaces
{
    public interface IEquatable : IValueInterface
    {
        public bool Equals(VariableValue value);
    }
}
