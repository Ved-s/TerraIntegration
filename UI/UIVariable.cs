using System;
using System.Collections.Generic;
using TerraIntegration.Basic;
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
        public virtual bool AcceptEmpty { get; set; } = false;

        public override ItemMatchDelegate ItemValidator => (item) =>
        {
            if (item.type != ModContent.ItemType<Items.Variable>()) return false;
            Variable var = (item.ModItem as Items.Variable)?.Var;
            if (var is null && !AcceptEmpty) return false;

            return VariableValidator?.Invoke(var) ?? true;
        };

        public override Item Item
        {
            get => Var?.Item;
            set => Var = value?.ModItem as Items.Variable;
        }

        public override void OnHover()
        {
            if (Var is null && VariableValidator is not null)
                ModContent.GetInstance<ComponentWorld>().VariableHighlights.Add(VariableValidator);
        }
    }
    public delegate bool VariableMatchDelegate(Variable var);
}
