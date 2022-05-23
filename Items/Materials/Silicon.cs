using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria.ModLoader;

namespace TerraIntegration.Items.Materials
{
    public class Silicon : ModItem
    {
        public override string Texture => "TerraIntegration/Assets/Items/Silicon";

        public override void SetDefaults()
        {
            Item.width = 12;
            Item.height = 12;
            Item.maxStack = 256;

            Item.value = 50;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddTile(TileID.Furnaces)
                .AddIngredient(ItemID.SandBlock, 5)
                .Register();
        }
    }
}
