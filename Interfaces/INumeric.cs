using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerraIntegration.Values;

namespace TerraIntegration.Interfaces
{
    public interface INumeric
    {
        public long NumericValue { get; }
        public long NumericMax { get; }
        public long NumericMin { get; }

        public VariableValue FromNumeric(long value, List<Error> errors);
    }
}
