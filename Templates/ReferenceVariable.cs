using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TerraIntegration.Basic;
using TerraIntegration.DataStructures;
using TerraIntegration.UI;
using TerraIntegration.Values;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace TerraIntegration.Templates
{
    public abstract class ReferenceVariable : Variable, IOwnProgrammerInterface
    {
        public Guid VariableId { get; set; }
        public virtual Type[] ReferenceReturnTypes { get; }

        public UIPanel Interface { get; set; }
        public UIVariableSlot InterfaceSlot { get; set; }
        public bool HasComplexInterface => false;

        public override Type[] RelatedTypes => ReferenceReturnTypes;

        public void SetupInterface()
        {
            InterfaceSlot = new()
            {
                DisplayOnly = true,
                Top = new(-21, .5f),
                Left = new(-21, .5f),
            };
            if (ReferenceReturnTypes is not null)
            {
                InterfaceSlot.VariableValidator = (var) => ReferenceReturnTypes.Any(t => t.IsAssignableFrom(var.VariableReturnType));
                InterfaceSlot.HoverText = string.Join(", ", ReferenceReturnTypes.Select(t => VariableValue.TypeToName(t, true)));
            }

            Interface.Append(InterfaceSlot);
        }
        public Variable WriteVariable()
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

        public abstract VariableValue GetValue(VariableValue value, ComponentSystem system, List<Error> errors);

        public override VariableValue GetValue(ComponentSystem system, List<Error> errors)
        {
            VariableValue val = system.GetVariableValue(VariableId, errors);
            if (val is null) return null;
            return GetValue(val, system, errors);
        }
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
