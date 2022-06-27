using TerraIntegration.Basic;
using TerraIntegration.Interfaces;
using TerraIntegration.ValueProperties;

namespace TerraIntegration.Values.Objects
{
    public class Item : VariableValue, ITyped, INamed
    {
        public override string TypeName => "item";
        public override string TypeDefaultDisplayName => "Item";
        public override string TypeDefaultDescription => "Represents an Item";

        public Terraria.Item ItemObj { get; set; }
        public int Type => ItemObj.type;
        public string Name => ItemObj.Name;

        public override void OnRegister()
        {
            AutoProperty<Item, Integer>.Register(new("stack", "Stack", (sys, item, err) => new(item.ItemObj.stack)));
        }

        public override bool Equals(VariableValue value)
        {
            return value is Item item &&
                item.ItemObj.type == ItemObj.type &&
                item.ItemObj.stack == item.ItemObj.stack &&
                Util.ObjectsNullEqual(item.ItemObj.ModItem, ItemObj.ModItem);
        }
    }
}
