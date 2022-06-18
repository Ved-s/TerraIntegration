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
        ReturnType[] ValidMathValueTypes { get; }

        ReturnType[] IAddable.ValidAddTypes => ValidMathValueTypes;
        ReturnType[] ISubtractable.ValidSubtractTypes => ValidMathValueTypes;
        ReturnType[] IMultipliable.ValidMultiplyTypes => ValidMathValueTypes;
        ReturnType[] IDivisible.ValidDivideTypes => ValidMathValueTypes;
        ReturnType[] IModulable.ValidModuloTypes => ValidMathValueTypes;
    }
}
