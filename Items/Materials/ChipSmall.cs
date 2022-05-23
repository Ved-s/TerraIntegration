using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria.ModLoader;

namespace TerraIntegration.Items.Materials
{
    internal class ChipSmall : ModItem
    {
        public override string Texture => "TerraIntegration/Assets/Items/ChipSmall";

        public override void SetDefaults()
        {
            Item.width = 10;
            Item.height = 10;
            Item.maxStack = 256;

            Item.value = 45;
        }

        public override void AddRecipes()
        {
            CreateRecipe(16)
                .AddTile(TileID.Sawmill)
                .AddTile(TileID.Furnaces)
                .AddIngredient<SiliconPlate>()
                .AddIngredient<CrystallizedSap>(8)
                .Register();
        }
    }
}
