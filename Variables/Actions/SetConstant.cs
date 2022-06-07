using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TerraIntegration.Basic;
using TerraIntegration.DataStructures;
using TerraIntegration.Items;
using TerraIntegration.Templates;

namespace TerraIntegration.Variables.Actions
{
    public class SetConstant : ActionWithReferenceConst
    {
        public override VariableMatch Variables => VariableMatch.OfType<Constant>();
        public override bool HasComplexInterface => false;

        public override string TypeName => "setconst";
        public override string TypeDefaultDisplayName => "Set Constant";

        public override SpriteSheetPos SpriteSheetPos => new(ActionsSheet, 1, 0);

        public override void Execute(Point16 pos, Basic.Variable var, VariableValue refValue, ComponentSystem system, List<Error> errors)
        {
            if (var is not Constant @const) return;
            @const.Value = refValue.Clone();

            VariableLocation? loc = var.CurrentLocation;
            if (loc.HasValue)
            {
                loc.Value.ComponentData.Component?.OnVariableChanged(loc.Value.ComponentPos, loc.Value.Slot);
            }
        }
        public override Type[] GetValidRightSlotTypes(Type leftSlotType) => new[] { leftSlotType };
    }
}
