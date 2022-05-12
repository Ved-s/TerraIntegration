using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using TerraIntegration.Variables;
using Terraria;
using Terraria.Audio;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;

namespace TerraIntegration.UI
{
    public abstract class UIVariable : UIElement
    {
        const int Context = ItemSlot.Context.InventoryCoin;
        float Scale = 0.8f;

        private static Item Air = new();

        public bool DisplayOnly { get; set; } = false;
        public IEnumerable<string> HighlightTypes { get; set; }
        public IEnumerable<Type> HighlightReturnTypes { get; set; }

        public abstract Items.Variable Var { get; set; }

        public Func<Variable, bool> VariableValidator { get; set; }

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

        public UIVariable()
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
                Items.Variable v = Var;

                Main.LocalPlayer.mouseInterface = true;

                if (v is not null)
                {
                    ModContent.GetInstance<ComponentWorld>().HoverItem = v.Item;
                }
                else 
                {
                    if (HighlightTypes is not null)
                        ModContent.GetInstance<ComponentWorld>().TypeHighlights.UnionWith(HighlightTypes);

                    if (HighlightReturnTypes is not null)
                        ModContent.GetInstance<ComponentWorld>().ReturnTypeHighlights.UnionWith(HighlightReturnTypes);
                }

                if (PlayerInput.MouseInfo.LeftButton == ButtonState.Pressed && PlayerInput.MouseInfoOld.LeftButton == ButtonState.Released)
                {
                    Item playerHeld = PlayerHeldItem;

                    if (playerHeld.type == ModContent.ItemType<Items.Variable>() && VariableValidator is not null) 
                    {
                        Variable var = (playerHeld.ModItem as Items.Variable).Var;
                        if (!VariableValidator(var)) return;
                    }

                    if (playerHeld.type == ModContent.ItemType<Items.Variable>() && playerHeld.stack == 1)
                    {
                        Items.Variable newVar = playerHeld.ModItem as Items.Variable;

                        if (DisplayOnly)
                        {
                            Var = playerHeld.Clone().ModItem as Items.Variable;
                        }
                        else
                        {
                            PlayerHeldItem = v?.Item ?? new();
                            Var = newVar;
                        }

                        SoundEngine.PlaySound(SoundID.Grab);
                    }
                    else if (v is null && playerHeld.type == ModContent.ItemType<Items.Variable>() && playerHeld.stack > 1) 
                    {
                        Item i = playerHeld.Clone();
                        i.stack = 1;

                        Items.Variable newVar = i.ModItem as Items.Variable;

                        if (!DisplayOnly) PlayerHeldItem.stack--;
                        Var = newVar;

                        SoundEngine.PlaySound(SoundID.Grab);
                    }

                    else if (v is not null && PlayerHeldItem.IsAir)
                    {
                        if (!DisplayOnly)
                        {
                            Item outItem = v.Item;
                            if (Main.playerInventory)
                                Main.mouseItem = outItem;
                            else Main.LocalPlayer.inventory[Main.LocalPlayer.selectedItem] = outItem;
                        }

                        Var = null;

                        SoundEngine.PlaySound(SoundID.Grab);
                    }
                }
            }
        }

        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            

            Vector2 position = GetDimensions().Position();
            Item i = Var?.Item ?? Air;

            float invScale = Main.inventoryScale;
            Main.inventoryScale = Scale;

            ItemSlot.Draw(spriteBatch, ref i, Context, position, default(Color));

            Main.inventoryScale = invScale;
        }

        public override void Update(GameTime gameTime)
        {
            HandleItemSlotLogic();
        }
    }
}
