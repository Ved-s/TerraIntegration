using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
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

        public virtual int MaxSlotCapacity { get; set; } = int.MaxValue;

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
                Item playerHeld = PlayerHeldItem;
                Item item = Item;

                if (PlayerInput.MouseInfo.LeftButton == ButtonState.Pressed && PlayerInput.MouseInfoOld.LeftButton == ButtonState.Released)
                {
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
                    else
                    {
                        if (ItemValidator is not null && !ItemValidator(playerHeld)) return;

                        if (!DisplayOnly && item is not null && CheckItemStack(item, playerHeld) &&
                            (item.stack < MaxSlotCapacity || playerHeld.stack < playerHeld.maxStack))
                        {
                            int itemMaxStack = Math.Min(item.maxStack, MaxSlotCapacity);

                            if (item.stack < itemMaxStack)
                            {
                                int take = Math.Min(playerHeld.stack, itemMaxStack - item.stack);

                                Item.stack += take;
                                playerHeld.stack -= take;
                                if (playerHeld.stack <= 0)
                                    playerHeld.TurnToAir();
                                PlayerHeldItem = playerHeld;
                            }
                            else
                            {
                                int give = Math.Min(item.stack, playerHeld.maxStack - item.stack);

                                item.stack -= give;
                                playerHeld.stack += give;
                                if (item.stack <= 0)
                                    item = null;
                                Item = item;
                            }
                            SoundEngine.PlaySound(SoundID.Grab);
                        }

                        else if (playerHeld.stack <= MaxSlotCapacity)
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
                            int take = Math.Min(playerHeld.stack, MaxSlotCapacity);
                            bool takeAll = take == playerHeld.stack;

                            if (takeAll)
                            {
                                Item = playerHeld;
                                playerHeld = new();
                            }
                            else
                            {
                                Item i = playerHeld.Clone();
                                i.stack = take;
                                Item = i;
                            }

                            if (!DisplayOnly)
                            {
                                if (!takeAll)
                                {
                                    playerHeld.stack -= take;
                                    if (playerHeld.stack <= 0)
                                        playerHeld.TurnToAir();
                                }

                                PlayerHeldItem = playerHeld;
                            }

                            SoundEngine.PlaySound(SoundID.Grab);
                        }
                    }
                }
                else if (Main.stackSplit <= 1 && Main.mouseRight && item is not null &&
                    (playerHeld.IsAir || CheckItemStack(playerHeld, item) && playerHeld.stack < playerHeld.maxStack))
                {
                    if (playerHeld.IsAir)
                    {
                        playerHeld = item.Clone();
                        playerHeld.stack = 0;
                    }

                    playerHeld.stack++;
                    item.stack--;

                    if (item.stack <= 0) item = null;

                    PlayerHeldItem = playerHeld;
                    Item = item;

                    SoundEngine.PlaySound(SoundID.MenuTick);
                    ItemSlot.RefreshStackSplitCooldown();

                }
            }
        }

        internal bool CheckItemStack(Item a, Item b)
        {
            return a.netID == b.netID && a.type == b.type && ItemLoader.CanStack(a, b);
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
