using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;

namespace TerraIntegration.Items
{
    public class Bluewood : ModItem
    {
        public override string Texture => "TerraIntegration/Assets/Items/Bluewood";

        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.maxStack = 4096;
            Item.useTurn = true;
            Item.autoReuse = true;
            Item.useAnimation = 10;
            Item.useTime = 10;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.consumable = true;
            Item.rare = ItemRarityID.White;
            Item.value = Item.sellPrice(copper: 20);
            Item.createTile = ModContent.TileType<Tiles.Bluewood>();
        }
    }
}
