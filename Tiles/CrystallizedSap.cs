using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace TerraIntegration.Tiles
{
    public class CrystallizedSap : ModTile
    {
        public override string Texture => "TerraIntegration/Assets/Tiles/CrystallizedSap";

        public override void SetStaticDefaults()
        {
            Main.tileFrameImportant[Type] = true;
            Main.tileMergeDirt[Type] = false;
        }

        public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak)
        {
            if (!noBreak)
            {
                Tile t = Main.tile[i, j];
                ushort type = ModContent.GetInstance<global::TerraIntegration.Bluewood>().Tile.Type;

                if (t.TileFrameY == 0  && Main.tile[i, j + 1].TileType != type 
                    || t.TileFrameY == 18 && Main.tile[i + 1, j].TileType != type 
                    || t.TileFrameY == 36 && Main.tile[i - 1, j].TileType != type)
                    WorldGen.KillTile(i, j);
            }
            return false;
        }
    }
}