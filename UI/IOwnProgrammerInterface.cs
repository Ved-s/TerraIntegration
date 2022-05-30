using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerraIntegration.Basic;
using TerraIntegration.DataStructures;
using TerraIntegration.Variables;
using Terraria.GameContent.UI.Elements;

namespace TerraIntegration.UI
{
    public interface IOwnProgrammerInterface
    {
        UIPanel Interface { get; set; }

        public void SetupInterfaceIfNeeded() 
        {
            if (Interface is null)
            {
                Interface = new()
                {
                    PaddingTop = 0,
                    PaddingBottom = 0,
                    PaddingLeft = 0,
                    PaddingRight = 0,
                };
                SetupInterface();
            }
        }

        void SetupInterface();

        Variable WriteVariable();
    }
}
