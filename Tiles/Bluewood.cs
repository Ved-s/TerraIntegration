using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace TerraIntegration.Tiles
{
    public class Bluewood : ModTile
    {
        public override string Texture => "TerraIntegration/Assets/Tiles/Bluewood";

        public override void SetStaticDefaults()
        {
            Main.tileSolid[Type] = true;
            Main.tileFrameImportant[Type] = false;
            Main.tileMergeDirt[Type] = false;

            ItemDrop = ModContent.ItemType<Items.Bluewood>();
        }
    }
}