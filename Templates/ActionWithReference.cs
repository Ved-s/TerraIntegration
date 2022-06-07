using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TerraIntegration.Basic;
using TerraIntegration.DataStructures;
using TerraIntegration.UI;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace TerraIntegration.Templates
{
    public abstract class ActionWithReference : ActionVariable
    {
        public Guid ReferenceId { get; set; }
        public override bool NeedsSaveTag => true;

        public override bool HasComplexInterface => false;

        public UIVariableSlot ReferenceSlot { get; set; }

        public virtual UIDrawing CenterDrawing => new UIDrawing()
        {
            OnDraw = (e, sb, style) =>
            {
                VariableRenderer.DrawVariableOverlay(sb, false, null, TypeName, style.Position() - new Vector2(16), new(32), Color.White, 0f, Vector2.Zero);
            }
        };

        public override void SaveActionData(BinaryWriter writer)
        {
            writer.Write(ReferenceId.ToByteArray());
        }
        public override ActionVariable LoadActionData(BinaryReader reader)
        {
            ActionWithReference awr = this.NewInstance();
            awr.ReferenceId = new(reader.ReadBytes(16));
            return awr;
        }

        public override TagCompound SaveActionTag()
        {
            return new()
            {
                ["rid"] = ReferenceId.ToByteArray()
            };
        }
        public override ActionVariable LoadActionTag(TagCompound data)
        {
            ActionWithReference sec = this.NewInstance();

            if (data.ContainsKey("rid"))
                sec.ReferenceId = new(data.GetByteArray("rid"));

            return sec;
        }

        public override void SetupActionInterface()
        {
            ActionVarSlot.Left.Set(-75, .5f);
            ActionVarSlot.VariableChanged = var =>
            {
                if (var?.Var is null || ReferenceSlot?.Var?.Var is not null && ReferenceSlot.Var.Var.VariableReturnType != var.Var.VariableReturnType)
                    ReferenceSlot.Var = null;

                if (var?.Var is null)
                {
                    ReferenceSlot.VariableValidator = (var) => false;
                    ReferenceSlot.HoverText = null;
                }
                else
                {
                    Type[] types = GetValidReferenceSlotTypes(var.Var.VariableReturnType);

                    ReferenceSlot.HoverText = var?.Var is null ? null :
                        string.Join(", ", types.Select(t => VariableValue.TypeToName(t, true)));
                    ReferenceSlot.VariableValidator = var => var?.VariableReturnType is not null && types.Any(t => var.VariableReturnType.IsAssignableTo(t));
                }
            };

            UIDrawing drawing = CenterDrawing;

            if (drawing is not null)
            {
                drawing.Top = new(0, .5f);
                drawing.Left = new(0, .5f);
                Interface.Append(drawing);
            }

            Interface.Append(ReferenceSlot = new()
            {
                Top = new(-21, .5f),
                Left = new(30, .5f),

                DisplayOnly = true,

                VariableValidator = var => ActionVarSlot?.Var?.Var is not null && var.VariableReturnType == ActionVarSlot.Var.Var.VariableReturnType
            });
        }
        public override ActionVariable WriteActionvariable()
        {
            if (ReferenceSlot?.Var?.Var is null)
                return null;

            ActionWithReference awr = CreateVariable(ReferenceSlot.Var.Var);
            if (awr is null) return null;

            awr.ReferenceId = ReferenceSlot.Var.Var.Id;
            return awr;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            if (ActionVarId != default)
                tooltips.Add(new(Mod, "TIActRefIds", $"[c/aaaa00:Variable IDs:] {World.Guids.GetShortGuid(ActionVarId)}, {World.Guids.GetShortGuid(ReferenceId)}"));
        }

        public override void Execute(Point16 pos, Variable var, ComponentSystem system, List<Error> errors)
        {
            VariableValue value = system.GetVariableValue(ReferenceId, errors);
            if (value is null) return;

            Execute(pos, var, value, system, errors);
        }

        public virtual ActionWithReference CreateVariable(Variable refVar) => this.NewInstance();

        public abstract Type[] GetValidReferenceSlotTypes(Type leftSlotType);
        public abstract void Execute(Point16 pos, Variable var, VariableValue refValue, ComponentSystem system, List<Error> errors);
    }
}
