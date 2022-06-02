using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerraIntegration.DataStructures;
using Terraria;
using Terraria.Audio;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace TerraIntegration.Components
{
    public class TileReader : Component
    {
        public override string ComponentType => "tile";
        public override string ComponentDisplayName => "Tile reader";

        public override SpriteSheet DefaultPropertySpriteSheet { get; set; } = new("TerraIntegration/Assets/Types/tile", new(32, 32));

        public override Vector2 InterfaceOffset => new(24, 0);

        public override void SetStaticDefaults()
        {
            SetupNewTile();

            Main.tileFrameImportant[Type] = true;
            Main.tileSolid[Type] = true;

            TileObjectData.newTile.Direction = TileObjectDirection.PlaceRight;
            TileObjectData.newTile.StyleWrapLimit = 4;
            TileObjectData.newTile.RandomStyleRange = 1;
            TileObjectData.newTile.StyleMultiplier = 1;
            TileObjectData.newTile.StyleHorizontal = true;

            TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile);
            TileObjectData.newAlternate.Direction = TileObjectDirection.PlaceLeft;
            TileObjectData.addAlternate(1);
            TileObjectData.addTile(Type);

            ItemDrop = ModContent.ItemType<Items.ComponentItems.TileReader>();
        }

        public override bool Slope(int i, int j)
        {
            Tile t = Framing.GetTileSafely(i, j);
            int frame = t.TileFrameX / 18;

            frame = frame switch
            {
                0 => 3,
                1 => 2,
                2 => 0,
                3 => 1,
                _ => 0
            };
            t.TileFrameX = (short)(frame * 18);

            SoundEngine.PlaySound(SoundID.Dig);

            return false;
        }

        public Tile GetTargetTile(Point16 pos) => Framing.GetTileSafely(GetTargetTilePos(pos));
        public Point16 GetTargetTilePos(Point16 pos) => GetTargetTilePos(pos.X, pos.Y);
        public Point16 GetTargetTilePos(int x, int y)
        {
            Tile t = Framing.GetTileSafely(x, y);
            int frame = t.TileFrameX / 18;

            switch (frame) 
            {
                case 0: return new(x+1, y);
                case 1: return new(x-1, y);
                case 2: return new(x, y-1);
                case 3: return new(x, y+1);
            }
            return default;
        }
    }
}
