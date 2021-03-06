using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace TerraIntegration.Items
{
    public class Cable : ModItem
    {
        public override string Texture => $"{nameof(TerraIntegration)}/Assets/Items/{Name}";

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Cable");
        }

        public override void SetDefaults()
        {
            Item.width = 20;
            Item.height = 20;
            Item.maxStack = 512;
            Item.useTurn = true;
            Item.autoReuse = true;
            Item.useAnimation = 10;
            Item.useTime = 10;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.consumable = true;
            Item.rare = ItemRarityID.White;
            Item.value = Item.sellPrice(copper: 20);
            Item.createTile = ModContent.TileType<Tiles.Cable>();
        }

        public override void AddRecipes()
        {
            CreateRecipe(16)
                .AddTile(TileID.WorkBenches)
                .AddIngredient<Materials.Bluewood>(4)
                .AddIngredient<Materials.CrystallizedSap>(8)
                .Register();

            CreateRecipe()
                .AddIngredient<CableWall>()
                .Register();
        }
    }
}
