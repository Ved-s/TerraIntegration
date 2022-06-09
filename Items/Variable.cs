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
using TerraIntegration.Basic;
using TerraIntegration.DataStructures;
using TerraIntegration.Values;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace TerraIntegration.Items
{
    public class Variable : ModItem
    {
        public override string Texture => $"{nameof(TerraIntegration)}/Assets/Items/{Name}";

        private const string VarTagKey = "var";
        public Basic.Variable Var = null;
        public byte Highlight;

        public override bool IsCloneable => true;

        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.maxStack = 256;
        }

        public override ModItem Clone(Item item)
        {
            Variable var = (Variable)base.Clone(item);
            var.Var = Var?.Clone();
            return var;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            string itemName = Var?.ItemName;
            if (itemName is not null)
            {
                TooltipLine line = tooltips.FirstOrDefault(t => t.Name == "ItemName");
                if (line is not null)
                {
                    string stack = "";
                    if (Item.stack > 1)
                        stack = $" ({Item.stack})";

                    line.Text = itemName + stack;
                }
            }

            if (Var is null) return;

            string returns = VariableValue.TypeToName(Var.VariableReturnType, true);

            tooltips.Add(new(Mod, "TIVarType", $"[c/aaaa00:Type:] {Var.TypeDisplayName}"));
            if (returns is not null)
                tooltips.Add(new(Mod, "TIVarReturn", $"[c/aaaa00:Returns:] {returns}"));
            tooltips.Add(new(Mod, "TIVarID", $"[c/aaaa00:ID:] {Var.ShortId}"));

            Var.ModifyTooltips(tooltips);

            if (Var.ShowLastValue && Var.LastValue is not null && Var.LastSystem is not null)
            {
                string text = Var.LastValue.Display(Var.LastSystem)?.HoverText;
                if (text is not null)
                    tooltips.Add(new(Mod, "TIVarLastVal", "[c/aaaa00:Last Value:] " + text.Replace('\n', ' ')));
            }

            if (Var.TypeDescription is not null)
            {
                tooltips.Add(new(Mod, "TIVarDescription", Var.TypeDescription));
            }
        }

        public override bool CanStack(Item item2)
        {
            if (Var is not null) return false;

            Variable var = item2.ModItem as Variable;
            if (var is null) return false;

            return var.Var is null;
        }

        public override void SaveData(TagCompound tag)
        {
            MemoryStream stream = new();
            BinaryWriter writer = new(stream);

            tag[VarTagKey] = Basic.Variable.SaveTag(Var);

            writer.Dispose();
        }

        public override void LoadData(TagCompound tag)
        {
            tag = GetUnloadedItemData(tag);
            if (tag.ContainsKey(VarTagKey))
                Var = Basic.Variable.LoadTag(tag.GetCompound(VarTagKey));
        }

        public override void NetSend(BinaryWriter writer)
        {
            Basic.Variable.SaveData(Var, writer);
        }

        public override void NetReceive(BinaryReader reader)
        {
            Var = Basic.Variable.LoadData(reader);
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddTile(TileID.WorkBenches)
                .AddIngredient<Materials.Bluewood>()
                .AddIngredient<Materials.CrystallizedSap>(4)
                .AddIngredient<Materials.ChipSmall>()
                .Register();
        }

        public static Item CreateVarItem(Basic.Variable var) 
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
            if (Var is null) return;

            Vector2 overlaySize = sourceRectangle.Size() * scale;
            VariableRenderer.DrawVariableOverlay(spriteBatch, false, Var.VariableReturnType, Var.TypeName, position, overlaySize, color, rotation, origin);
        }

        TagCompound GetUnloadedItemData(TagCompound tag)
        {
            if (tag.ContainsKey("mod") && tag.ContainsKey("name") && tag.ContainsKey("data")) 
            {
                return GetUnloadedItemData(tag.GetCompound("data"));
            }
            return tag;
        }
    }
}
