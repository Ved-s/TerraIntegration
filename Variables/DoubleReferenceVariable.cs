using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerraIntegration.DataStructures;
using TerraIntegration.UI;
using TerraIntegration.Values;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader;

namespace TerraIntegration.Variables
{
    public abstract class DoubleReferenceVariable : Variable, IOwnProgrammerInterface
    {
        public UIPanel Interface { get; set; }

        public UIVariableSlot LeftSlot { get; set; }
        public UIVariableSlot RightSlot { get; set; }

        public abstract ValueMatcher LeftSlotValueTypes { get; }
        public virtual UIDrawing CenterDrawing => new UIDrawing()
        {
            OnDraw = (e, sb, style) =>
            {
                VariableRenderer.DrawVariableOverlay(sb, false, Type, style.Position() - new Vector2(16), new(32), Color.White, 0f, Vector2.Zero);
            }
        };

        public Guid LeftId { get; set; }
        public Guid RightId { get; set; }

        private ValueMatcher ValidRightTypes;
        private Dictionary<ReturnValue, ValueMatcher> ValidTypesCache = new();
        private HashSet<(ReturnValue, ReturnValue)> ValidTypePairs = new();
        
        public void SetupInterface()
        {
            Interface.Append(LeftSlot = new()
            {
                Top = new(-21, .5f),
                Left = new(-75, .5f),

                DisplayOnly = true,
                VariableValidator = (var) => LeftSlotValueTypes.Match(var.VariableReturnType),
                HoverText = LeftSlotValueTypes.MatchTypes is null ? null : string.Join(", ", LeftSlotValueTypes.MatchTypes.Select(t => VariableValue.TypeToName(t, true))),

                VariableChanged = (var) =>
                {
                    if (var?.Var.VariableReturnType is null)
                    {
                        ValidRightTypes = ValueMatcher.MatchNone;
                        RightSlot.HoverText = null;
                        return;
                    }

                    ValidRightTypes = GetValidRightSlotTypes(var.Var.VariableReturnType);
                    if (ValidRightTypes.MatchesNone)
                    {
                        RightSlot.HoverText = null;
                        RightSlot.Var = null;
                        return;
                    }

                    if (RightSlot.Var is not null && !ValidRightTypes.Match(RightSlot.Var.Var.VariableReturnType))
                        RightSlot.Var = null;

                    RightSlot.HoverText = ValidRightTypes.MatchTypes is null ? null :
                        string.Join(", ", ValidRightTypes.MatchTypes.Select(t => VariableValue.TypeToName(t, true)));
                }
            });

            UIDrawing drawing = CenterDrawing;

            if (drawing is not null)
            {
                drawing.Top = new(0, .5f);
                drawing.Left = new(0, .5f);
                Interface.Append(drawing);
            }

            Interface.Append(RightSlot = new()
            {
                Top = new(-21, .5f),
                Left = new(30, .5f),

                DisplayOnly = true,
                VariableValidator = (var) => ValidRightTypes.Match(var.VariableReturnType),
            });
        }

        public Variable WriteVariable()
        {
            if (LeftSlot?.Var?.Var is null || RightSlot?.Var?.Var is null) return null;

            DoubleReferenceVariable doubleRef = CreateVariable(LeftSlot.Var.Var, RightSlot.Var.Var);

            if (doubleRef is null) return null;

            doubleRef.LeftId = LeftSlot.Var.Var.Id;
            doubleRef.RightId = RightSlot.Var.Var.Id;

            return doubleRef;
        }

        public override VariableValue GetValue(ComponentSystem system, List<Error> errors)
        {
            VariableValue left = system.GetVariableValue(LeftId, errors);
            VariableValue right = system.GetVariableValue(RightId, errors);

            if (left is null || right is null) return null;
            ReturnValue leftType = left.GetReturnValue();
            ReturnValue rightType = right.GetReturnValue();

            if (!ValidTypePairs.Contains((leftType, rightType)))
            {
                if (!LeftSlotValueTypes.Match(leftType))
                {
                    errors.Add(new(
                        ErrorType.ExpectedValuesWithId,
                        string.Join(", ", LeftSlotValueTypes.MatchTypes?.Select(t => VariableValue.TypeToName(t, false)) ?? Array.Empty<string>()),
                        World.Guids.GetShortGuid(Id)));
                    return null;
                }

                if (!ValidTypesCache.TryGetValue(leftType, out ValueMatcher validRightTypes))
                {
                    validRightTypes = GetValidRightSlotTypes(leftType);
                    ValidTypesCache[leftType] = validRightTypes;
                }

                if (!validRightTypes.Match(rightType))
                {
                    errors.Add(new(
                        ErrorType.ExpectedValuesWithId,
                        string.Join(", ", validRightTypes.MatchTypes?.Select(t => VariableValue.TypeToName(t, false)) ?? Array.Empty<string>()),
                        World.Guids.GetShortGuid(Id)));
                    return null;
                }

                ValidTypePairs.Add((leftType, rightType));
            }

            return GetValue(system, left, right, errors);
        }
        protected override void SaveCustomData(BinaryWriter writer)
        {
            writer.Write(LeftId.ToByteArray());
            writer.Write(RightId.ToByteArray());
        }
        protected override Variable LoadCustomData(BinaryReader reader)
        {
            DoubleReferenceVariable doubleRef = (DoubleReferenceVariable)Activator.CreateInstance(GetType());

            doubleRef.LeftId = new(reader.ReadBytes(16));
            doubleRef.RightId = new(reader.ReadBytes(16));

            return doubleRef;
        }
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            if (LeftId != default && RightId != default)
                tooltips.Add(new(Mod, "TIDRefIds", $"[c/aaaa00:Referenced IDs:] {World.Guids.GetShortGuid(LeftId)}, {World.Guids.GetShortGuid(RightId)}"));
        }

        public abstract VariableValue GetValue(ComponentSystem system, VariableValue left, VariableValue right, List<Error> errors);
        public abstract ValueMatcher GetValidRightSlotTypes(ReturnValue? leftSlotReturn);

        public virtual DoubleReferenceVariable CreateVariable(Variable left, Variable right) => (DoubleReferenceVariable)Activator.CreateInstance(GetType());
    }
}
