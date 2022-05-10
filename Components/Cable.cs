using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace TerraIntegration.Components
{
    public class Cable : Component
    {
        public override string ComponentType => "cable";

		public override bool ShouldSaveData(ComponentData data) => false;

        public override void SetStaticDefaults()
        {
			Main.tileSolid[Type] = false;
			SetupNewTile();
			TileObjectData.addTile(Type);

			ItemDrop = ModContent.ItemType<Items.Cable>();
		}

		public static int CanPlace(int i, int j, int type, int style, int direction, int alternative)
		{
			return 0;
		}

		public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak)
        {
			short x = 0, y = 0;

			if (CanConnectTo(Main.tile[i, j - 1].TileType)) y += 18;
			if (CanConnectTo(Main.tile[i - 1, j].TileType)) x += 18;
			if (CanConnectTo(Main.tile[i, j + 1].TileType)) y += 36;
			if (CanConnectTo(Main.tile[i + 1, j].TileType)) x += 36;

			Tile tile = Main.tile[i, j];
			tile.TileFrameX = x;
			tile.TileFrameY = y;

			return false;
        }

		private static bool CanConnectTo(int type)
		{
			return TileTypes.Contains(type);
		}
    }
}
