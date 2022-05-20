using System;
using System.Collections.Generic;
using TerraIntegration.Variables;
using Terraria;
using Terraria.ModLoader;

namespace TerraIntegration.UI
{
    public abstract class UIVariable : UIItem
    {
        public abstract Items.Variable Var { get; set; }

        public virtual VariableMatchDelegate VariableValidator { get; set; }

        public override int MaxSlotCapacity => 1;

        public override ItemMatchDelegate ItemValidator => (item) =>
            item.type == ModContent.ItemType<Items.Variable>() && (VariableValidator?.Invoke((item.ModItem as Items.Variable)?.Var) ?? true);

        public override Item Item
        {
            get => Var?.Item;
            set => Var = value?.ModItem as Items.Variable;
        }

        public override void OnHover()
        {
            if (Var is not null && VariableValidator is not null)
                ModContent.GetInstance<ComponentWorld>().VariableHighlights.Add(VariableValidator);
        }
    }
    public delegate bool VariableMatchDelegate(Variable var);
}
