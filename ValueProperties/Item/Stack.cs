using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerraIntegration.DataStructures;
using TerraIntegration.Templates;

namespace TerraIntegration.ValueProperties.Item
{
    public class ItemStack : ValueProperty<Values.Objects.Item, Values.Integer>
    {
        public override string PropertyName => "stack";
        public override string PropertyDisplay => "Stack";

        public override Values.Integer GetProperty(ComponentSystem system, Values.Objects.Item value, List<Error> errors)
        {
            return new(value.ItemObj.stack);
        }
    }
}
