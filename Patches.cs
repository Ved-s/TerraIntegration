using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;

namespace TerraIntegration
{
    public class Patches : ILoadable
    {
        public TerraIntegration Mod => ModContent.GetInstance<TerraIntegration>();

        public void Load(Mod mod)
        {
            IL.Terraria.Main.DrawItem += DrawItemTexturePatch;
            IL.Terraria.UI.ItemSlot.Draw_SpriteBatch_ItemArray_int_int_Vector2_Color += PostItemSlotBackgroundDrawPatch;

            On.Terraria.WorldGen.TileFrame += WorldGen_TileFrame;
            On.Terraria.WorldGen.KillTile += WorldGen_KillTile;
            On.Terraria.TileObject.Place += TileObject_Place;

            Terraria.IO.WorldFile.OnWorldLoad += WorldFile_OnWorldLoad;
        }
        public void Unload()
        {
            IL.Terraria.Main.DrawItem -= DrawItemTexturePatch;
            IL.Terraria.UI.ItemSlot.Draw_SpriteBatch_ItemArray_int_int_Vector2_Color -= PostItemSlotBackgroundDrawPatch;

            On.Terraria.WorldGen.TileFrame -= WorldGen_TileFrame;
            On.Terraria.WorldGen.KillTile -= WorldGen_KillTile;
            On.Terraria.TileObject.Place -= TileObject_Place;

            Terraria.IO.WorldFile.OnWorldLoad -= WorldFile_OnWorldLoad;
        }

        private void WorldGen_TileFrame(On.Terraria.WorldGen.orig_TileFrame orig, int i, int j, bool resetFrame, bool noBreak)
        {
            TileMimicking.BeforeTileFrame(i, j);
            orig(i, j, resetFrame, noBreak);
            TileMimicking.AfterTileFrame(i, j);
        }
        private void WorldGen_KillTile(On.Terraria.WorldGen.orig_KillTile orig, int i, int j, bool fail, bool effectOnly, bool noItem)
        {
            int type = Main.tile[i, j].TileType;
            orig(i, j, fail, effectOnly, noItem);
            if (!Main.tile[i, j].HasTile && Components.Component.TileTypes.Contains(type))
            {
                Components.Component.ByTileType[type].OnKilled(new(i, j));
            }
        }
        private bool TileObject_Place(On.Terraria.TileObject.orig_Place orig, TileObject toBePlaced)
        {
            bool result = orig(toBePlaced);
            if (Components.Component.ByTileType.TryGetValue(toBePlaced.type, out Components.Component c))
                c.OnPlaced(new(toBePlaced.xCoord, toBePlaced.yCoord));

            return result;
        }
        private void WorldFile_OnWorldLoad()
        {
            HashSet<Point16> updated = new();

            for (int y = 0; y < Main.maxTilesY; y++)
                for (int x = 0; x < Main.maxTilesX; x++)
                {
                    Tile t = Main.tile[x, y];
                    if (!t.HasTile) continue;
                    if (!Components.Component.TileTypes.Contains(t.TileType)) continue;
                    Components.Component.ByTileType[t.TileType].OnLoaded(new(x, y));
                    if (updated.Contains(new(x, y))) continue;

                    ComponentSystem system = ComponentSystem.UpdateSystem(new(x, y));
                    updated.UnionWith(system.ComponentsByPos.Keys);
                }
        }

        private void DrawItemTexturePatch(ILContext il)
        {
            ILCursor c = new(il);

            /*
              Args: Main instance, Item item, int whoAmI
             
              IL_07B9: ldsfld    class [FNA]Microsoft.Xna.Framework.Graphics.SpriteBatch Terraria.Main::spriteBatch
	          IL_07BE: ldloc.0           // texture
	          IL_07BF: ldloc.s   vector2 // position
	          IL_07C1: ldloc.1           // sourceRectangle

	          IL_07C2: newobj    instance void valuetype [System.Runtime]System.Nullable`1<valuetype [FNA]Microsoft.Xna.Framework.Rectangle>::.ctor(!0)
	          IL_07C7: ldloc.s   currentColor
	          IL_07C9: ldloc.s   num     // rotation
	          IL_07CB: ldloc.3           // origin

	          IL_07CC: ldloc.s   scale
	          IL_07CE: ldc.i4.0          // effects
	          IL_07CF: ldc.r4    0.0     // layerDepth
	          IL_07D4: callvirt  instance void [FNA]Microsoft.Xna.Framework.Graphics.SpriteBatch::Draw(class [FNA]Microsoft.Xna.Framework.Graphics.Texture2D, valuetype [FNA]Microsoft.Xna.Framework.Vector2, valuetype [System.Runtime]System.Nullable`1<valuetype [FNA]Microsoft.Xna.Framework.Rectangle>, valuetype [FNA]Microsoft.Xna.Framework.Color, float32, valuetype [FNA]Microsoft.Xna.Framework.Vector2, float32, valuetype [FNA]Microsoft.Xna.Framework.Graphics.SpriteEffects, float32)
             */

            int position = -1,
                sourceRectangle = -1,
                color = -1,
                rotation = -1,
                origin = -1,
                scale = -1;

            if (!c.TryGotoNext(MoveType.After,
                x => x.MatchLdsfld<Main>("spriteBatch"),
                x => x.MatchLdloc(out _),
                x => x.MatchLdloc(out position),
                x => x.MatchLdloc(out sourceRectangle),

                x => x.MatchNewobj(out _),
                x => x.MatchLdloc(out color),
                x => x.MatchLdloc(out rotation),
                x => x.MatchLdloc(out origin),

                x => x.MatchLdloc(out scale),
                x => x.MatchLdcI4(out _),
                x => x.MatchLdcR4(out _),
                x => x.MatchCallvirt<SpriteBatch>("Draw")
                ))
            {
                Mod.Logger.WarnFormat("Patch error: {0} (DrawItemTexturePatch)", il.Method.FullName);
                return;
            }


            c.Emit(OpCodes.Ldarg_1);
            c.Emit(OpCodes.Ldloc, position);
            c.Emit(OpCodes.Ldloc, sourceRectangle);
            c.Emit(OpCodes.Ldloc, color);
            c.Emit(OpCodes.Ldloc, rotation);
            c.Emit(OpCodes.Ldloc, origin);
            c.Emit(OpCodes.Ldloc, scale);
            c.Emit<Patches>(OpCodes.Call, nameof(PostDrawHook));
        }
        private void PostItemSlotBackgroundDrawPatch(ILContext il)
        {
            ILCursor c = new(il);
            /*
             Args: SpriteBatch spriteBatch, Item[] inv, int context, int slot, Vector2 position, Color lightColor 
             
             IL_0683: ldloc.s   flag2
	         IL_0685: brtrue.s  IL_06B3

	         IL_0687: ldarg.0
	         IL_0688: ldloc.s   'value'
	         IL_068A: ldarg.s   position
	         IL_068C: ldloca.s  V_24

	         IL_068E: initobj   valuetype [System.Runtime]System.Nullable`1<valuetype [FNA]Microsoft.Xna.Framework.Rectangle>
	         IL_0694: ldloc.s   V_24
	         IL_0696: ldloc.s   color2
	         IL_0698: ldc.r4    0.0

	         IL_069D: ldloca.s  V_25
	         IL_069F: initobj   [FNA]Microsoft.Xna.Framework.Vector2
	         IL_06A5: ldloc.s   V_25
	         IL_06A7: ldloc.2

	         IL_06A8: ldc.i4.0
	         IL_06A9: ldc.r4    0.0
	         IL_06AE: callvirt  instance void [FNA]Microsoft.Xna.Framework.Graphics.SpriteBatch::Draw(class [FNA]Microsoft.Xna.Framework.Graphics.Texture2D, valuetype [FNA]Microsoft.Xna.Framework.Vector2, valuetype [System.Runtime]System.Nullable`1<valuetype [FNA]Microsoft.Xna.Framework.Rectangle>, valuetype [FNA]Microsoft.Xna.Framework.Color, float32, valuetype [FNA]Microsoft.Xna.Framework.Vector2, float32, valuetype [FNA]Microsoft.Xna.Framework.Graphics.SpriteEffects, float32)
            */

            int texture = -1,
                scale = -1;

            if (!c.TryGotoNext(MoveType.After,

                x=>x.MatchLdloc(out _),
                x=>x.MatchBrtrue(out _),

                x=>x.MatchLdarg(0),
                x=>x.MatchLdloc(out texture),
                x=>x.MatchLdarg(4),
                x=>x.MatchLdloca(out _),

                x=>x.MatchInitobj(out _),
                x=>x.MatchLdloc(out _),
                x=>x.MatchLdloc(out _),
                x=>x.MatchLdcR4(out _),

                x=>x.MatchLdloca(out _),
                x=>x.MatchInitobj(out _),
                x=>x.MatchLdloc(out _),
                x=>x.MatchLdloc(out scale),

                x=>x.MatchLdcI4(out _),
                x=>x.MatchLdcR4(out _),
                x=>x.MatchCallvirt<SpriteBatch>("Draw")
                ))
            {
                Mod.Logger.WarnFormat("Patch error: {0} (" + nameof(PostItemSlotBackgroundDrawPatch) + ")");
                return;
            }

            c.Emit(OpCodes.Ldarg_0);
            c.Emit(OpCodes.Ldarg_1);
            c.Emit(OpCodes.Ldarg_3);
            c.Emit(OpCodes.Ldarg, 4);
            c.Emit(OpCodes.Ldloc, texture);
            c.Emit(OpCodes.Ldloc, scale);
            c.Emit<Patches>(OpCodes.Call, nameof(PostItemSlotPackgoundDrawHook));
        }

        public static void PostDrawHook(Item item, Vector2 position, Rectangle sourceRectangle, Color color, float rotation, Vector2 origin, float scale)
        {
            if (item.ModItem is Items.Variable var)
                var.PostDrawAll(Main.spriteBatch, position, sourceRectangle, color, rotation, origin, scale);
        }
        public static void PostItemSlotPackgoundDrawHook(SpriteBatch spriteBatch, Item[] inv, int slot, Vector2 position, Texture2D texture, float scale) 
        {
            if (slot >= inv.Length) return;

            ModContent.GetInstance<ComponentWorld>().HighlightItem(spriteBatch, inv[slot], texture, position, scale);
        }
    }
}
