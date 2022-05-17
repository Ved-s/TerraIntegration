using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Terraria;
using Terraria.Audio;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;

namespace TerraIntegration.UI
{
    public abstract class UIItem : UIElement
    {
        const int Context = ItemSlot.Context.InventoryCoin;
        private readonly static Item Air = new();

        public virtual bool DisplayOnly { get; set; } = false;
        public virtual float Scale { get; set; } = 0.8f;

        public abstract Item Item { get; set; }
        public virtual ItemMatchDelegate ItemValidator { get; set; }

        static Item PlayerHeldItem
        {
            get 
            {
                if (Main.playerInventory) return Main.mouseItem;
                return Main.LocalPlayer.inventory[Main.LocalPlayer.selectedItem];
            }
            set 
            {
                if (Main.playerInventory) Main.mouseItem = value;
                else Main.LocalPlayer.inventory[Main.LocalPlayer.selectedItem] = value;
            }
        }

        public UIItem()
        {
            Width = new StyleDimension(52 * Scale, 0f);
            Height = new StyleDimension(52 * Scale, 0f);

            MarginTop = 0;
            MarginLeft = 0;
            MarginRight = 0;
            MarginBottom = 0;
        }

        private void HandleItemSlotLogic()
        {
            if (IsMouseHovering)
            {
                Main.LocalPlayer.mouseInterface = true;

                if (Item is not null)
                {
                    ModContent.GetInstance<ComponentWorld>().HoverItem = Item;
                }

                OnHover();

                if (PlayerInput.MouseInfo.LeftButton == ButtonState.Pressed && PlayerInput.MouseInfoOld.LeftButton == ButtonState.Released)
                {
                    Item playerHeld = PlayerHeldItem;

                    if (PlayerHeldItem.IsAir)
                    {
                        if (Item is not null)
                        {
                            if (!DisplayOnly)
                            {
                                if (Main.playerInventory)
                                    Main.mouseItem = Item;
                                else Main.LocalPlayer.inventory[Main.LocalPlayer.selectedItem] = Item;
                            }

                            Item = null;

                            SoundEngine.PlaySound(SoundID.Grab);
                        }
                    }

                    else if (ItemValidator is not null && !ItemValidator(playerHeld)) return;

                    if (playerHeld.stack == 1)
                    {
                        if (DisplayOnly)
                        {
                            Item = playerHeld.Clone();
                        }
                        else
                        {
                            PlayerHeldItem = Item ?? new();
                            Item = playerHeld;
                        }

                        SoundEngine.PlaySound(SoundID.Grab);
                    }
                    else if (Item is null && playerHeld.stack > 1) 
                    {
                        Item i = playerHeld.Clone();
                        i.stack = 1;
                        Item = i;

                        if (!DisplayOnly) PlayerHeldItem.stack--;

                        SoundEngine.PlaySound(SoundID.Grab);
                    }
                }
            }
        }

        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            Vector2 position = GetDimensions().Position();
            Item i = Item ?? Air;

            float invScale = Main.inventoryScale;
            Main.inventoryScale = Scale;

            ItemSlot.Draw(spriteBatch, ref i, Context, position, default(Color));

            Main.inventoryScale = invScale;
        }

        public override void Update(GameTime gameTime)
        {
            HandleItemSlotLogic();
        }

        public virtual void OnHover() { }
    }

    public delegate bool ItemMatchDelegate(Item item);
}
