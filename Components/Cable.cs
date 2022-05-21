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

		public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak)
        {
			short x = 0, y = 0;

			if (CanConnectTo(TileMimicking.GetRealTileType(i, j - 1))) y += 18;
			if (CanConnectTo(TileMimicking.GetRealTileType(i - 1, j))) x += 18;
			if (CanConnectTo(TileMimicking.GetRealTileType(i, j + 1))) y += 36;
			if (CanConnectTo(TileMimicking.GetRealTileType(i + 1, j))) x += 36;

			Tile tile = Main.tile[i, j];
			tile.TileFrameX = x;
			tile.TileFrameY = y;

			return false;
        }

		private bool CanConnectTo(int type)
		{
			return type == Type || Components.Component.TileTypes.Contains(type);
		}
    }
}
