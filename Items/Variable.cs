using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerraIntegration.Values;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace TerraIntegration.Items
{
    public class Variable : ModItem
    {
        public override string Texture => $"{nameof(TerraIntegration)}/Assets/Items/{Name}";

        private const string VarTagKey = "var";
        public Variables.Variable Var = new();
        public byte Highlight;

        public static Dictionary<string, Asset<Texture2D>> VariableTypeOverlays = new();
        public static Dictionary<string, Asset<Texture2D>> VariableValueOverlays = new();

        const string TypeOverlayPath = "Assets/Types";
        const string ValueOverlayPath = "Assets/Values";

        public override void Load()
        {
            if (!Main.dedServ)
            {
                VariableValueOverlays.Clear();
                VariableTypeOverlays.Clear();
                foreach (string asset in Mod.RootContentSource.EnumerateAssets())
                {
                    if (asset.StartsWith(TypeOverlayPath) || asset.StartsWith(ValueOverlayPath))
                    {
                        string type = Path.GetFileNameWithoutExtension(asset);
                        string path = $"{Mod.Name}/{Path.ChangeExtension(asset, null)}";
                        Asset<Texture2D> tex = ModContent.Request<Texture2D>(path);

                        if (asset.StartsWith(ValueOverlayPath)) VariableValueOverlays[type] = tex;
                        else VariableTypeOverlays[type] = tex;
                    }
                }
            }
        }

        public override void Unload()
        {
            VariableValueOverlays.Clear();
            VariableTypeOverlays.Clear();
        }

        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.maxStack = 256;

            Var.Item = Item;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            base.ModifyTooltips(tooltips);

            if (Var.IsEmpty) return;

            string returns = VariableValue.TypeToName(Var.VariableReturnType, out Color c);
            returns = Util.ColorTag(c, returns);

            tooltips.Add(new(Mod, "TIVarType", $"[c/aaaa00:Type:] {Var.TypeDisplay}"));
            if (returns is not null)
                tooltips.Add(new(Mod, "TIVarReturn", $"[c/aaaa00:Returns:] {returns}"));
            tooltips.Add(new(Mod, "TIVarID", $"[c/aaaa00:ID:] {Var.ShortId}"));

            Var.ModifyTooltips(tooltips);

            if (Var.TypeDescription is not null)
            {
                tooltips.Add(new(Mod, "TIVarDescription", Var.TypeDescription));
            }
        }

        public override bool CanStack(Item item2)
        {
            if (!Var.IsEmpty) return false;

            Variable var = item2.ModItem as Variable;
            if (var is null) return false;

            return var.Var.IsEmpty;
        }

        public override void SaveData(TagCompound tag)
        {
            MemoryStream stream = new();
            BinaryWriter writer = new(stream);

            tag[VarTagKey] = Var.SaveTag();

            writer.Dispose();
        }

        public override void LoadData(TagCompound tag)
        {
            if (tag.ContainsKey(VarTagKey))
                Var = Variables.Variable.LoadTag(tag.GetCompound(VarTagKey)) ?? new();
            
        }

        public override void NetSend(BinaryWriter writer)
        {
            Var.SaveData(writer);
        }

        public override void NetReceive(BinaryReader reader)
        {
            Var = Variables.Variable.LoadData(reader);
        }

        public static Item CreateVarItem(Variables.Variable var) 
        {
            Item i = new();
            i.SetDefaults(ModContent.ItemType<Variable>());

            Variable v = i.ModItem as Variable;
            v.Var = var;

            return i;
        }

        public override void PostDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
        {
            PostDrawAll(spriteBatch, position, frame, drawColor, 0f, origin, scale);
        }

        public void PostDrawAll(SpriteBatch spriteBatch, Vector2 position, Rectangle sourceRectangle, Color color, float rotation, Vector2 origin, float scale) 
        {
            Vector2 overlaySize = sourceRectangle.Size() * scale;
            DrawVariableOverlay(spriteBatch, false, Var.VariableReturnType, Var.Type, position, overlaySize, color, rotation, origin);
        }

        public static void DrawVariableOverlay(SpriteBatch spriteBatch, bool drawVariable, Type returnType, string type, Vector2 pos, Vector2 size, Color color, float rotation, Vector2 origin)
        {
            if (Main.dedServ) return;

            if (drawVariable)
            {
                Asset<Texture2D> variable = TextureAssets.Item[ModContent.ItemType<Variable>()];

                if (!variable.IsLoaded)
                    variable.Wait();

                Vector2 scale = size / variable.Size();

                spriteBatch.Draw(variable.Value, pos, null, color, rotation, origin, scale, SpriteEffects.None, 0);
            }

            if (returnType is not null && Values.VariableValue.ByType.TryGetValue(returnType, out var val))
            {
                if (VariableValueOverlays.TryGetValue(val.Type.Replace('.', '_'), out var valueOverlay))
                {
                    if (!valueOverlay.IsLoaded)
                        valueOverlay.Wait();

                    Vector2 scale = size / valueOverlay.Size();

                    spriteBatch.Draw(valueOverlay.Value, pos, null, color, rotation, origin, scale, SpriteEffects.None, 0);
                }
            }

            string typeOrig = type;
            if (type is not null)
                type = type.Replace('.', '_');

            if (!VariableTypeOverlays.TryGetValue(type, out var typeOverlay))
            {
                if (Variables.Variable.ByTypeName.TryGetValue(typeOrig, out var variable) && variable is Variables.PropertyVariable pv) 
                {
                    if (!VariableTypeOverlays.TryGetValue(pv.ComponentType, out typeOverlay))
                        VariableTypeOverlays.TryGetValue(pv.ComponentProperty, out typeOverlay);
                }
            }

            if (typeOverlay is not null) 
            {
                if (!typeOverlay.IsLoaded)
                    typeOverlay.Wait();

                Vector2 scale = size / typeOverlay.Size();

                spriteBatch.Draw(typeOverlay.Value, pos, null, color, rotation, origin, scale, SpriteEffects.None, 0);
            }
        }
    }
}
