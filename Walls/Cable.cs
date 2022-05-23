using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace TerraIntegration.Walls
{
    public class Cable : ModWall
    {
        public override string Texture => "TerraIntegration/Assets/Walls/Cable";

        static List<(Point pos, Rectangle frame)> DelayedDraws = new();

        public override void SetStaticDefaults()
        {
            Main.wallHouse[Type] = true;

            ItemDrop = ModContent.ItemType<Items.CableWall>();
        }

        public override bool PreDraw(int i, int j, SpriteBatch spriteBatch)
        {
            short fx = 0, fy = 0;

            if (ComponentSystem.CableWalls.Contains(Framing.GetTileSafely(i, j - 1).WallType)) fy += 18;
            if (ComponentSystem.CableWalls.Contains(Framing.GetTileSafely(i - 1, j).WallType)) fx += 18;
            if (ComponentSystem.CableWalls.Contains(Framing.GetTileSafely(i, j + 1).WallType)) fy += 36;
            if (ComponentSystem.CableWalls.Contains(Framing.GetTileSafely(i + 1, j).WallType)) fx += 36;

            DelayedDraws.Add((new(i, j), new Rectangle(fx, fy, 16, 16)));

            return false;
        }

        internal static void DelayedDraw()
        {
            foreach (var (pos, frame) in DelayedDraws) 
            {
                Vector2 screenpos = Util.WorldToScreen(pos);

                Tile t = Main.tile[pos.X, pos.Y];

                Texture2D tex = Main.instance.TilePaintSystem.TryGetWallAndRequestIfNotReady(t.WallType, t.WallColor) ?? TextureAssets.Wall[t.WallType].Value;

                Lighting.GetCornerColors(pos.X, pos.Y, out var vertices, 1f);

                Main.tileBatch.Draw(tex, screenpos, frame, vertices, Vector2.Zero, 1f, SpriteEffects.None);
            }
            DelayedDraws.Clear();
        }
    }
}
