using Microsoft.Xna.Framework;
using System;
using Terraria;

namespace TerraIntegration.UI
{
    public class UIItemVirtual : UIItem
    {
        public Func<Item> GetItem { get; set; }
        public Action<Item> SetItem { get; set; }

        public override Item Item
        {
            get => GetItem();
            set => SetItem(value);
        }
    }
}
