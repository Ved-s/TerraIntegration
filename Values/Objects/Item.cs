using Microsoft.Xna.Framework;
using System.IO;
using TerraIntegration.Basic;
using TerraIntegration.DisplayedValues;
using TerraIntegration.Interfaces;
using TerraIntegration.Interfaces.Value;
using TerraIntegration.Variables;
using Terraria;
using Terraria.GameContent.UI;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.ModLoader.IO;

namespace TerraIntegration.Values.Objects
{
    public class Item : VariableValue, ITyped, INamed, IProgrammable
    {
        public override string TypeName => "item";
        public override string TypeDefaultDisplayName => "Item";
        public override string TypeDefaultDescription => "Represents an Item";

        public Terraria.Item ItemObj { get; set; }
        public int Type => ItemObj.type;
        public string Name => ItemObj.Name;

        public UI.UIItemSlot Slot;
        public UIPanel Interface { get; set; }
        public bool HasComplexInterface => false;

        public Item() { }
        public Item(Terraria.Item item)
        {
            ItemObj = item;
        }

        public override DisplayedValue Display(ComponentSystem system)
        {
            return new ItemDisplay(ItemObj);
        }

        protected override void SaveCustomData(BinaryWriter writer)
        {
            TagCompound tag = ItemIO.Save(ItemObj);
            MemoryStream ms = new();
            TagIO.ToStream(tag, ms);
            writer.Write((int)ms.Position);
            ms.Position = 0;
            ms.CopyTo(writer.BaseStream);
        }

        protected override VariableValue LoadCustomData(BinaryReader reader)
        {
            int len = reader.ReadInt32();
            byte[] bytes = reader.ReadBytes(len);
            MemoryStream ms = new(bytes);
            TagCompound tag = TagIO.FromStream(ms);
            return new Item(ItemIO.Load(tag));
        }

        public override bool Equals(VariableValue value)
        {
            return value is Item item &&
                item.ItemObj.type == ItemObj.type &&
                item.ItemObj.stack == item.ItemObj.stack &&
                Util.ObjectsNullEqual(item.ItemObj.ModItem, ItemObj.ModItem);
        }

        public void SetupInterface()
        {
            Interface.Append(Slot = new()
            {
                Top = new(-21, .5f),
                Left = new(-21, .5f),
                DisplayOnly = true,
            });
        }

        public Variable WriteVariable()
        {
            if (Slot?.Item is null)
            {
                Slot?.NewFloatingText(TerraIntegration.Localize("ProgrammingErrors.NoItem"), Color.Red);
                return null;
            }
            return new Constant(new Item(Slot.Item));
        }
    }
}
