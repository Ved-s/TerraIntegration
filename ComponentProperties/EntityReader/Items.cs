using Microsoft.Xna.Framework;
using System.Collections.Generic;
using TerraIntegration.Basic;
using TerraIntegration.DataStructures;
using TerraIntegration.Interfaces;
using Terraria;

namespace TerraIntegration.ComponentProperties.EntityReader
{
    public class Items : ComponentProperty<Components.EntityReader>
    {
        public override string PropertyName => "items";
        public override string PropertyDisplay => "Items";

        public override ReturnType? VariableReturnType => new(typeof(ICollection), typeof(Values.Objects.Item));

        public List<Values.Objects.Item> FoundItems = new();

        public override VariableValue GetProperty(Components.EntityReader component, Point16 pos, List<Error> errors)
        {
            FoundItems.Clear();

            Rectangle scannerRect = new(pos.X * 16, pos.Y * 16, 16, 16);

            for (int i = 0; i < Main.item.Length; i++)
            {
                Item item = Main.item[i];
                if (item.active && item.getRect().Intersects(scannerRect))
                    FoundItems.Add(new(item));
            }

            return FoundItems.ToCollectionValue();
        }
    }
}
