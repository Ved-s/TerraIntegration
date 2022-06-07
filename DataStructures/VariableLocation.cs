using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerraIntegration.Basic;

namespace TerraIntegration.DataStructures
{
    public struct VariableLocation
    {
        public Point16 ComponentPos;
        public string Slot;

        public ComponentData ComponentData;
    }
}
