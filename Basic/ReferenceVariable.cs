using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using TerraIntegration.UI;
using TerraIntegration.Values;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace TerraIntegration.Basic
{
    public abstract class ReferenceVariable : Variable, IOwnProgrammerInterface
    {
        public Guid VariableId { get; set; }
        public virtual Type ReferenceReturnType { get; }

        public UIPanel Interface { get; set; }
        public UIVariableSlot InterfaceSlot { get; set; }

        public void SetupInterface()
        {
            InterfaceSlot = new()
            {
                DisplayOnly = true,
                Top = new(-21, .5f),
                Left = new(-21, .5f),
            };
            if (ReferenceReturnType is not null)
            {
                InterfaceSlot.VariableValidator = (var) => ReferenceReturnType?.IsAssignableFrom(var.VariableReturnType) ?? false;
                InterfaceSlot.HoverText = VariableValue.TypeToName(ReferenceReturnType, true);
            }

            Interface.Append(InterfaceSlot);
        }
        public Basic.Variable WriteVariable()
        {
            if (InterfaceSlot.Var is not null)
            {
                ReferenceVariable v = CreateVariable(InterfaceSlot.Var.Var);
                if (v is not null)
                {
                    v.VariableId = InterfaceSlot.Var.Var.Id;
                    return v;
                }
            }
            return null;
        }

        public virtual ReferenceVariable CreateVariable(Variable var) => (ReferenceVariable)Activator.CreateInstance(GetType());

        protected override void SaveCustomData(BinaryWriter writer)
        {
            writer.Write(VariableId.ToByteArray());
        }
        protected override Variable LoadCustomData(BinaryReader reader)
        {
            ReferenceVariable refvar = (ReferenceVariable)Activator.CreateInstance(GetType());

            refvar.VariableId = new(reader.ReadBytes(16));
            return refvar;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            base.ModifyTooltips(tooltips);

            if (VariableId != default)
                tooltips.Add(new(Mod, "TIRefVarId", $"[c/aaaa00:Variable ID:] {World.Guids.GetShortGuid(VariableId)}"));
        }
    }
}
