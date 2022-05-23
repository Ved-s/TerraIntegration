using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;
using Terraria;

namespace TerraIntegration.Items.Materials
{
    public class CrystallizedSap : ModItem
    {
        public override string Texture => "TerraIntegration/Assets/Items/CrystallizedSap";

        public override void SetStaticDefaults()
        {
            Tooltip.SetDefault("Crystallized Bluewood sap");
        }

        public override void SetDefaults()
        {
            Item.width = 10;
            Item.height = 10;
            Item.maxStack = 512;
            Item.value = Item.sellPrice(copper: 45);
        }
    }
}
