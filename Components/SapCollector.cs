using CustomTreeLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace TerraIntegration.Components
{
    public class SapCollector : Component
    {
        public override string Texture => "TerraIntegration/Assets/Tiles/Camo";

        public override string ComponentType => "sap";

        public override void SetStaticDefaults()
        {
            Main.tileSolid[Type] = false;
            Main.tileFrameImportant[Type] = true;

            SetupNewTile();
            TileObjectData.addTile(Type);

            ItemDrop = ModContent.ItemType<Items.SapCollector>();
        }

        public override bool CanPlace(int i, int j)
        {
            return IsBluewoodAround(i, j);
        }

        public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak)
        {
            if (!noBreak && !IsBluewoodAround(i,j))
                WorldGen.KillTile(i, j);

            return false;
        }

        public bool IsBluewoodAround(int x, int y) 
        {
            ushort bluewood = ModContent.GetInstance<Bluewood>().Tile.Type;

            if (Main.tile[x-1, y].TileType == bluewood && TreeTileInfo.GetInfo(x-1, y).IsCenter)
                return true;

            if (Main.tile[x+1, y].TileType == bluewood && TreeTileInfo.GetInfo(x+1, y).IsCenter)
                return true;

            return false;
        }
    }
}
