using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria.ModLoader;

namespace TerraIntegration.Items.Materials
{
    internal class Chip : ModItem
    {
        public override string Texture => "TerraIntegration/Assets/Items/Chip";

        public override void SetDefaults()
        {
            Item.width = 16;
            Item.height = 16;
            Item.maxStack = 256;

            Item.value = 60;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddTile(TileID.Sawmill)
                .AddTile(TileID.Furnaces)
                .AddIngredient<SiliconPlate>()
                .AddIngredient<CrystallizedSap>(4)
                .Register();
        }
    }
}
