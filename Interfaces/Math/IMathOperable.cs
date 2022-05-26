using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TerraIntegration.Interfaces.Math
{
    public interface IMathOperable : IAddable, ISubtractable, IMultipliable, IDivisible, IModulable
    {
        Type[] ValidMathValueTypes { get; }

        Type[] IAddable.ValidAddTypes => ValidMathValueTypes;
        Type[] ISubtractable.ValidSubtractTypes => ValidMathValueTypes;
        Type[] IMultipliable.ValidMultiplyTypes => ValidMathValueTypes;
        Type[] IDivisible.ValidDivideTypes => ValidMathValueTypes;
        Type[] IModulable.ValidModuloTypes => ValidMathValueTypes;
    }
}
