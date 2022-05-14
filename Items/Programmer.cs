using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerraIntegration.UI;
using Terraria;
using Terraria.ModLoader;

namespace TerraIntegration.Items
{
    public class Programmer : ModItem
    {
        public override string Texture => $"{nameof(TerraIntegration)}/Assets/Items/{Name}";

        public override void SetDefaults()
        {
            Item.width = 48;
            Item.height = 48;
            Item.maxStack = 1;
        }

        public override bool ConsumeItem(Player player) => false;
        public override bool CanRightClick() => true;
        public override void RightClick(Player player) => ProgrammerInterface.Toggle();
    }
}
