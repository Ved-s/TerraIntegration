using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerraIntegration.Basic;
using TerraIntegration.DataStructures;
using TerraIntegration.UI;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader;
using Terraria.ObjectData;
using Terraria.UI;

namespace TerraIntegration.Components
{
    public class VarStore : Component
    {
        public override string ComponentType => "store";
        public override string ComponentDisplayName => "Variable store";
        public override string ComponentDescription => "Stores up to 16 variables.";

        public override bool CanHaveVariables => true;

        public override bool HasCustomInterface => true;

        public override Vector2 InterfaceOffset => new(24, 0);

        public const string SlotNames = "0123456789abcdef";

        public UIComponentVariable[] Slots = new UIComponentVariable[16];

        public override void SetStaticDefaults()
        {
            Main.tileSolid[Type] = false;
            Main.tileFrameImportant[Type] = true;
            SetupNewTile();
            TileObjectData.addTile(Type);

            ItemDrop = ModContent.ItemType<Items.ComponentItems.VarStore>();
        }

        public override UIPanel SetupInterface()
        {
            UIPanel panel = new()
            {
                Width = new(214, 0),
                Height = new(214, 0),
                PaddingTop = 0,
                PaddingLeft = 0,
                PaddingRight = 0,
                PaddingBottom = 0,
            };

            for (int i = 0; i < Slots.Length; i++)
            {
                int x = i % 4;
                int y = i / 4;

                x *= 52;
                y *= 52;

                x += 8;
                y += 8;

                UIComponentVariable slot = new UIComponentVariable()
                {
                    Top = new(y, 0),
                    Left = new(x, 0),

                    VariableSlot = SlotNames[i].ToString(),
                };
                Slots[i] = slot;

                panel.Append(slot);

            }

            return panel;
        }

        public override void UpdateInterface(Point16 pos)
        {
            PositionedComponent c = new(pos, this);

            foreach (UIComponentVariable slot in Slots)
                slot.Component = c;
        }
    }
}
