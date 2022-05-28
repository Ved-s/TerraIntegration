using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerraIntegration.DataStructures;

namespace TerraIntegration.Interfaces.Math
{
    public interface IMathOperable : IAddable, ISubtractable, IMultipliable, IDivisible, IModulable
    {
        ValueMatcher ValidMathValueTypes { get; }

        ValueMatcher IAddable.ValidAddTypes => ValidMathValueTypes;
        ValueMatcher ISubtractable.ValidSubtractTypes => ValidMathValueTypes;
        ValueMatcher IMultipliable.ValidMultiplyTypes => ValidMathValueTypes;
        ValueMatcher IDivisible.ValidDivideTypes => ValidMathValueTypes;
        ValueMatcher IModulable.ValidModuloTypes => ValidMathValueTypes;
    }
}
