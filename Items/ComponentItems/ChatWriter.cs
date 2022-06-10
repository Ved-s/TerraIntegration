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
    public class ChatWriter : ModItem
    {
        public override string Texture => "TerraIntegration/Assets/Items/ChatWriter";

        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.maxStack = 128;
            Item.useTurn = true;
            Item.autoReuse = true;
            Item.useAnimation = 10;
            Item.useTime = 10;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.consumable = true;
            Item.rare = ItemRarityID.White;
            Item.value = Item.sellPrice(silver: 10);
            Item.createTile = ModContent.TileType<Components.ChatWriter>();
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddTile(TileID.WorkBenches)
                .AddIngredient<Materials.Bluewood>(10)
                .AddIngredient<Materials.ChipSmall>(2)
                .AddIngredient<Materials.CrystallizedSap>(5)
                .AddIngredient(ItemID.AnnouncementBox, 1)
                .Register();
        }
    }
}