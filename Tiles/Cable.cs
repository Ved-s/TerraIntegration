using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerraIntegration.Basic;
using TerraIntegration.Utilities;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace TerraIntegration.Tiles
{
    public class Cable : ModTile
    {
        public override string Texture => "TerraIntegration/Assets/Tiles/Cable";

        public override void SetStaticDefaults()
        {
			Main.tileSolid[Type] = false;
			TileObjectData.newTile.Width = 1;
			TileObjectData.newTile.Height = 1;
			TileObjectData.newTile.Origin = new(0, 0);
			TileObjectData.newTile.CoordinateHeights = new[] { 16 };
			TileObjectData.newTile.CoordinateWidth = 16;
			TileObjectData.newTile.CoordinatePadding = 2;
			TileObjectData.newTile.HookCheckIfCanPlace = new PlacementHook((_, _, _, _, _, _) => 0, -1, 0, true);
			TileObjectData.newTile.UsesCustomCanPlace = true;
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
			return ComponentSystem.CableTiles.Contains(type) || Component.TileTypes.Contains(type);
		}
    }
}
