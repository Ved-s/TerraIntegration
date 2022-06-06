using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerraIntegration.Basic;
using TerraIntegration.DataStructures;
using TerraIntegration.Items;
using TerraIntegration.UI;
using Terraria.ModLoader;

namespace TerraIntegration.Variables.Actions
{
    public class SetConstant : ActionVariable
    {
        public override VariableMatch Variables => VariableMatch.OfType<Constant>();
        public override bool HasComplexInterface => false;

        public override string TypeName => "setconst";
        public override string TypeDefaultDisplayName => "Set Constant";

        public Guid NewValueId { get; set; }
        public UIVariableSlot NewValueSlot { get; set; }
        public override bool NeedsSaveTag => false;

        public override void Execute(Basic.Variable var, ComponentSystem system, List<Error> errors)
        {
            if (var is not Constant @const) return;

            VariableValue value = system.GetVariableValue(NewValueId, errors);
            if (value is null) return;

            @const.Value = value;
        }

        public override void SaveActionData(BinaryWriter writer)
        {
            writer.Write(NewValueId.ToByteArray());
        }
        public override ActionVariable LoadActionData(BinaryReader reader)
        {
            return new SetConstant() { NewValueId = new(reader.ReadBytes(16)) };
        }

        public override void SetupActionInterface()
        {
            ActionVarSlot.Left.Set(-52, .5f);
            ActionVarSlot.VariableChanged = var =>
            {
                if (var?.Var is null || NewValueSlot?.Var?.Var is not null && NewValueSlot.Var.Var.VariableReturnType != var.Var.VariableReturnType)
                    NewValueSlot.Var = null;

                NewValueSlot.HoverText = var?.Var is null ? null : VariableValue.TypeToName(var.Var.VariableReturnType, true);
            };

            Interface.Append(NewValueSlot = new()
            {
                Top = new(-21, .5f),
                Left = new(10, .5f),

                DisplayOnly = true,

                VariableValidator = var => ActionVarSlot?.Var?.Var is not null && var.VariableReturnType == ActionVarSlot.Var.Var.VariableReturnType
            });
        }
        public override ActionVariable WriteActionvariable()
        {
            if (NewValueSlot?.Var?.Var is null)
                return null;
            return new SetConstant() { NewValueId = NewValueSlot.Var.Var.Id };
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            if (ActionVarId != default)
                tooltips.Add(new(Mod, "TISetConstTargetId", $"[c/aaaa00:Constant ID:] {World.Guids.GetShortGuid(ActionVarId)}"));
            if (NewValueId != default)
                tooltips.Add(new(Mod, "TISetConstValueId", $"[c/aaaa00:Value ID:] {World.Guids.GetShortGuid(NewValueId)}"));
        }
    }
}
