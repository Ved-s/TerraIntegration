using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria.ModLoader;

namespace TerraIntegration.Items.Materials
{
    internal class SiliconPlate : ModItem
    {
        public override string Texture => "TerraIntegration/Assets/Items/SiliconPlate";

        public override void SetDefaults()
        {
            Item.width = 20;
            Item.height = 12;
            Item.maxStack = 256;

            Item.value = 30;
        }

        public override void AddRecipes()
        {
            CreateRecipe(2)
                .AddTile(TileID.Sawmill)
                .AddIngredient<Silicon>()
                .Register();
        }
    }
}
