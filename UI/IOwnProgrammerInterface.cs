using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.GameContent.UI.Elements;

namespace TerraIntegration.UI
{
    public interface IOwnProgrammerInterface
    {
        UIPanel Interface { get; set; }

        public void SetupInterfaceIfNeeded() 
        {
            if (Interface is null)
                SetupInterface();
        }

        void SetupInterface();

        void WriteVariable(Items.Variable var);
    }
}
