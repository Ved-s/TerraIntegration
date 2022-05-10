using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
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
        }

        public void Unload()
        {
            IL.Terraria.Main.DrawItem -= DrawItemTexturePatch;
        }

        private void DrawItemTexturePatch(MonoMod.Cil.ILContext il)
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
                x=>x.MatchLdsfld<Main>("spriteBatch"),
                x=>x.MatchLdloc(out _),
                x=>x.MatchLdloc(out position),
                x=>x.MatchLdloc(out sourceRectangle),

                x=>x.MatchNewobj(out _),
                x=>x.MatchLdloc(out color),
                x=>x.MatchLdloc(out rotation),
                x=>x.MatchLdloc(out origin),

                x=>x.MatchLdloc(out scale),
                x=>x.MatchLdcI4(out _),
                x=>x.MatchLdcR4(out _),
                x=>x.MatchCallvirt<SpriteBatch>("Draw")
                )) 
            {
                Mod.Logger.WarnFormat("Patch error: {0} (texture draw hook)", il.Method.FullName);
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

        public static void PostDrawHook(Item item, Vector2 position, Rectangle sourceRectangle, Color color, float rotation, Vector2 origin, float scale)
        {
            if (item.ModItem is Items.Variable var)
                var.PostDrawAll(Main.spriteBatch, position, sourceRectangle, color, rotation, origin, scale);
        }
    }
}
