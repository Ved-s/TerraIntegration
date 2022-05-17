using Microsoft.Xna.Framework;
using System;
using Terraria;

namespace TerraIntegration.UI
{
    public class UIItemSlot : UIItem
    {
        private Item item = null;

        public override Item Item
        {
            get => item;
            set
            {
                item = value;
                ItemChanged?.Invoke();
            }
        }
        public Point WorldPos { get; set; }

        public event Action ItemChanged;

        public override void OnDeactivate()
        {
            if (Item is null || DisplayOnly) return;

            Util.DropItemInWorld(Item, WorldPos.X, WorldPos.Y);
            Item = null;
        }
    }
}
