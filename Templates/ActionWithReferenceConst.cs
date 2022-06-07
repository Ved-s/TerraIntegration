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
    public abstract class ActionWithReferenceConst : ActionVariable
    {
        public ValueOrRef RightValue { get; set; }
        public override bool NeedsSaveTag => true;

        public override bool HasComplexInterface => true;

        public UIConstantOrReference RightSlot { get; set; }

        public virtual UIDrawing CenterDrawing => new UIDrawing()
        {
            OnDraw = (e, sb, style) =>
            {
                VariableRenderer.DrawVariableOverlay(sb, false, null, TypeName, style.Position() - new Vector2(16), new(32), Color.White, 0f, Vector2.Zero);
            }
        };

        public override void SaveActionData(BinaryWriter writer)
        {
            RightValue.SaveData(writer);
        }
        public override ActionVariable LoadActionData(BinaryReader reader)
        {
            ActionWithReferenceConst awr = this.NewInstance();
            awr.RightValue = ValueOrRef.LoadData(reader);
            return awr;
        }

        public override TagCompound SaveActionTag()
        {
            TagCompound tag = new();
            if (RightValue is not null)
            {
                if (RightValue.IsRef)
                    tag["rid"] = RightValue.RefId.ToByteArray();
                else
                    tag["rv"] = Util.WriteToByteArray(w => VariableValue.SaveData(RightValue.Value, w));
            }
            return tag;
        }
        public override ActionVariable LoadActionTag(TagCompound data)
        {
            ActionWithReferenceConst awr = this.NewInstance();

            if (data.ContainsKey("rid"))
                awr.RightValue = new(new Guid(data.GetByteArray("rid")));
            else if (data.ContainsKey("rv"))
                awr.RightValue = new(Util.ReadFromByteArray(data.GetByteArray("rv"), VariableValue.LoadData));

            return awr;
        }

        public override void SetupActionInterface()
        {
            ActionVarSlot.Left.Set(-75, .5f);
            ActionVarSlot.VariableChanged = var =>
            {
                if (var?.Var is null)
                {
                    RightSlot.ValidRefTypes = null;
                    RightSlot.ValidConstTypes = null;
                }
                else
                {
                    RightSlot.ValidRefTypes = GetValidRightReferenceSlotTypes(var.Var.VariableReturnType);
                    RightSlot.ValidConstTypes = GetValidRightConstantSlotTypes(var.Var.VariableReturnType);
                }
            };

            UIDrawing drawing = CenterDrawing;

            if (drawing is not null)
            {
                drawing.Top = new(0, .5f);
                drawing.Left = new(0, .5f);
                Interface.Append(drawing);
            }

            Interface.Append(RightSlot = new()
            {
                Top = new(-59, .5f),
                Left = new(30, .5f),
                BackgroundColor = Color.Transparent,
                BorderColor = Color.Transparent,
            });
        }
        public override ActionVariable WriteActionvariable()
        {
            ValueOrRef right = RightSlot.GetValue();

            if (right is null)
                return null;

            ActionWithReferenceConst awr = CreateVariable(right);
            if (awr is null) return null;

            awr.RightValue = right;
            return awr;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            if (ActionVarId != default)
                tooltips.Add(new(Mod, "TIActRefIds", $"[c/aaaa00:Variables:] Ref {World.Guids.GetShortGuid(ActionVarId)}, {RightValue}"));
        }

        public override void Execute(Point16 pos, Variable var, ComponentSystem system, List<Error> errors)
        {
            VariableValue value = RightValue?.GetValue(system, errors);
            if (value is null) return;

            Execute(pos, var, value, system, errors);
        }

        public virtual ActionWithReferenceConst CreateVariable(ValueOrRef right) => this.NewInstance();

        public virtual Type[] GetValidRightReferenceSlotTypes(Type leftSlotType) => GetValidRightSlotTypes(leftSlotType);
        public virtual Type[] GetValidRightConstantSlotTypes(Type leftSlotType) => GetValidRightSlotTypes(leftSlotType);
        public virtual Type[] GetValidRightSlotTypes(Type leftSlotType) => null;

        public abstract void Execute(Point16 pos, Variable var, VariableValue refValue, ComponentSystem system, List<Error> errors);
    }
}
