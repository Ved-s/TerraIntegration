using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerraIntegration.Basic;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.UI;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.UI;
using Terraria.UI.Chat;

namespace TerraIntegration.DisplayedValues
{
    public class ItemDisplay : DisplayedValue
    {
        public override string Type => "item";
        public override string HoverText => Util.ColorTag(GetRarityColor(Item), Item.HoverName);
        public override string ShortHoverText => HoverText;

        public Item Item { get; }

        public ItemDisplay() { }
        public ItemDisplay(Item item) 
        {
            Item = item ?? throw new ArgumentNullException(nameof(item));
        }

        public override void Draw(Rectangle screenRect, SpriteBatch spriteBatch)
        {
            Vector2 position = screenRect.Location.ToVector2();
            Color color = Color.White;
            float maxScale = 1f;

            // Partially copy-pasted from ItemSlot.Draw
            Main.instance.LoadItem(Item.type);
            Texture2D tex = TextureAssets.Item[Item.type].Value;
            Rectangle frame = (Main.itemAnimations[Item.type] == null) ? tex.Frame(1, 1, 0, 0, 0, 0) : Main.itemAnimations[Item.type].GetFrame(tex, -1);
            Color currentColor = color;
            float scale = 1f;
            ItemSlot.GetItemLight(ref currentColor, ref scale, Item, false);
            
            Vector2 size = new Vector2(frame.Width, frame.Height);

            float fixedScale = Math.Min(screenRect.Width / size.X, screenRect.Height / size.Y);
            scale *= Math.Min(fixedScale, scale * maxScale);
            fixedScale = Math.Min(fixedScale, maxScale);

            position += (screenRect.Size() - scale * size) / 2;
            if (ItemLoader.PreDrawInInventory(Item, spriteBatch, position, frame, Item.GetAlpha(currentColor), Item.GetColor(color), default, scale))
{
                spriteBatch.Draw(tex, position, frame, Item.GetAlpha(currentColor), 0f, default, scale, 0, 0f);
            }
            ItemLoader.PostDrawInInventory(Item, spriteBatch, position, frame, Item.GetAlpha(currentColor), Item.GetColor(color), default, scale);

            if (Item.stack > 1)
            {
                position = (screenRect.Size() - fixedScale * size) / 2;
                TextSnippet[] text = ChatManager.ParseMessage(Item.stack.ToString(), Color.White).ToArray();
                Vector2 textScale = new Vector2(.9f) * fixedScale;
                Vector2 textSize = ChatManager.GetStringSize(FontAssets.ItemStack.Value, text, Vector2.One) - new Vector2(0, 8);

                position += size - textSize * textScale;
                position += new Vector2(3, 6);

                position.X = Math.Min(position.X, screenRect.Width - textSize.X);
                position.Y = Math.Min(position.Y, screenRect.Height - textSize.Y);

                ChatManager.DrawColorCodedStringWithShadow(spriteBatch, FontAssets.ItemStack.Value, text, position + screenRect.Location.ToVector2(), 0f, Vector2.Zero, textScale, out _, -1f, 1f);
            }
        }

        public override bool Equals(DisplayedValue value)
        {
            return value is ItemDisplay item &&
                item.Item.type == Item.type &&
                item.Item.stack == item.Item.stack &&
                Util.ObjectsNullEqual(item.Item.ModItem, Item.ModItem);
        }

        protected override DisplayedValue ReceiveCustomData(BinaryReader reader)
        {
            return new ItemDisplay(ItemIO.Receive(reader, true));
        }

        protected override void SendCustomData(BinaryWriter writer)
        {
            ItemIO.Send(Item, writer, true);
        }

        static Color GetRarityColor(Terraria.Item item)
        {
            if (item.expert || item.rare == ItemRarityID.Expert)
            {
                return new(Main.DiscoR, Main.DiscoG, Main.DiscoB);
            }
            else if (item.master || item.rare == ItemRarityID.Master)
            {
                return new(255, (byte)(Main.masterColor * 200f), 0);
            }
            return ItemRarity.GetColor(item.rare);
        }
    }
}
