using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace TerraIntegration.Items.ComponentItems
{
    public class ActionBox : ModItem
    {
        public override string Texture => "TerraIntegration/Assets/Tiles/ActionBox";

        public override void SetDefaults()
        {
            Item.width = 20;
            Item.height = 20;
            Item.maxStack = 256;
            Item.useTurn = true;
            Item.autoReuse = true;
            Item.useAnimation = 10;
            Item.useTime = 10;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.consumable = true;
            Item.rare = ItemRarityID.White;
            Item.value = Item.sellPrice(silver: 10);
            Item.createTile = ModContent.TileType<Components.ActionBox>();
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddTile(TileID.WorkBenches)
                .AddIngredient<Materials.Chip>(2)
                .AddIngredient(ItemID.Chest)
                .Register();
        }
    }
}