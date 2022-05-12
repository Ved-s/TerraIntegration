using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Mono.Cecil.Cil;
using MonoMod.Cil;
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
        }

        public void Unload()
        {
            IL.Terraria.Main.DrawItem -= DrawItemTexturePatch;
            IL.Terraria.UI.ItemSlot.Draw_SpriteBatch_ItemArray_int_int_Vector2_Color -= PostItemSlotBackgroundDrawPatch;
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
