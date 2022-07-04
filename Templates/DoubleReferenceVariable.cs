using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerraIntegration.Basic;
using TerraIntegration.DataStructures;
using TerraIntegration.Interfaces;
using TerraIntegration.UI;
using TerraIntegration.Utilities;
using TerraIntegration.Values;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace TerraIntegration.Templates
{
    public abstract class DoubleReferenceVariable : Variable, IProgrammable
    {
        public UIPanel Interface { get; set; }

        public UIVariableSlot LeftSlot { get; set; }
        public UIVariableSlot RightSlot { get; set; }

        public virtual string LeftSlotDescription => null;
        public virtual string RightSlotDescription => null;

        public virtual bool RightSlotOptional => false;

        public abstract ReturnType[] LeftSlotValueTypes { get; }
        public virtual UIDrawing CenterDrawing => new UIDrawing()
        {
            OnDraw = (e, sb, style) =>
            {
                VariableRenderer.DrawVariableOverlay(sb, false, null, TypeName, style.Position() - new Vector2(16), new(32), Color.White, 0f, Vector2.Zero);
            }
        };

        public Guid LeftId { get; set; }
        public Guid RightId { get; set; }
        public bool HasComplexInterface => false;

        public override IEnumerable<Type> RelatedTypes => LeftSlotValueTypes.Select(rt => rt.Type);

        private ReturnType[] ValidRightTypes;
        private Dictionary<ReturnType, ReturnType[]> ValidTypesCache = new();
        private HashSet<(ReturnType, ReturnType?)> ValidTypePairs = new();

        public void SetupInterface()
        {
            Interface.Append(LeftSlot = new()
            {
                Top = new(-21, .5f),
                Left = new(-75, .5f),

                DisplayOnly = true,
                VariableValidator = (var) => LeftSlotValueTypes is not null && LeftSlotValueTypes.Any(t => t.Match(var.VariableReturnType)),
                HoverText = TypeListWithDescription(LeftSlotValueTypes, LeftSlotDescription, false),

                VariableChanged = (var) =>
                {
                    if (var?.Var.VariableReturnType is null)
                    {
                        ValidRightTypes = null;
                        RightSlot.HoverText = null;
                        return;
                    }

                    ValidRightTypes = GetValidRightSlotTypes(var.Var.VariableReturnType.Value);
                    if (ValidRightTypes is null)
                    {
                        ValidRightTypes = null;
                        RightSlot.HoverText = RightSlotDescription;
                        RightSlot.Var = null;
                        return;
                    }

                    if (RightSlot.Var is not null && !ValidRightTypes.Any(t => t.Match(RightSlot.Var.Var.VariableReturnType)))
                        RightSlot.Var = null;

                    RightSlot.HoverText = TypeListWithDescription(ValidRightTypes, RightSlotDescription, RightSlotOptional);
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
                VariableValidator = (var) => ValidRightTypes is not null && ValidRightTypes.Any(t => t.Match(var.VariableReturnType)),
                HoverText = RightSlotDescription
            });
        }

        public Variable WriteVariable()
        {
            if (LeftSlot is not null && LeftSlot.Var?.Var is null)
            {
                LeftSlot.NewFloatingText(TerraIntegration.Localize("ProgrammingErrors.NoVariable"), Color.Red);
                return null;
            }

            if (!RightSlotOptional && RightSlot is not null && RightSlot.Var?.Var is null)
            {
                RightSlot.NewFloatingText(TerraIntegration.Localize("ProgrammingErrors.NoVariable"), Color.Red);
                return null;
            }

            DoubleReferenceVariable doubleRef = CreateVariable(LeftSlot.Var.Var, RightSlot?.Var?.Var);

            if (doubleRef is null) return null;

            doubleRef.LeftId = LeftSlot.Var.Var.Id;
            doubleRef.RightId = RightSlot?.Var?.Var?.Id ?? default;

            return doubleRef;
        }

        public string TypeListWithDescription(IEnumerable<ReturnType> types, string description, bool optional)
        {
            if (types is null) return description;

            string result = string.Join(", ", string.Join(", ", types.Select(t => t.ToStringName(true))));

            if (description is not null)
                result += "\n" + description;

            if (optional)
                result = "[Optional] " + result;

            return result;
        }

        public override VariableValue GetValue(ComponentSystem system, List<Error> errors)
        {
            VariableValue left = system.GetVariableValue(LeftId, errors);
            VariableValue right = RightId == default ? null : system.GetVariableValue(RightId, errors);

            if (left is null || (!RightSlotOptional && right is null)) return null;
            ReturnType leftType = left.GetReturnType();
            ReturnType? rightType = right?.GetReturnType();

            if (!ValidTypePairs.Contains((leftType, rightType)))
            {
                if (LeftSlotValueTypes is not null && !LeftSlotValueTypes.Any(t => t.Match(leftType)))
                {
                    errors.Add(Errors.ExpectedValues(LeftSlotValueTypes, TypeIdentity));
                    return null;
                }
                if (rightType is not null)
                {
                    if (!ValidTypesCache.TryGetValue(leftType, out ReturnType[] validRightTypes))
                    {
                        validRightTypes = GetValidRightSlotTypes(leftType);
                        ValidTypesCache[leftType] = validRightTypes;
                    }

                    if (validRightTypes is not null && !validRightTypes.Any(t => t.Match(rightType)))
                    {
                        errors.Add(Errors.ExpectedValues(validRightTypes, TypeIdentity));
                        return null;
                    }
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

        protected override TagCompound SaveCustomTag()
        {
            return new()
            {
                ["rid"] = RightId.ToByteArray(),
                ["lid"] = LeftId.ToByteArray(),
            };
        }
        protected override Variable LoadCustomTag(TagCompound data)
        {
            DoubleReferenceVariable doubleRef = this.NewInstance();

            if (data.ContainsKey("rid"))
                doubleRef.RightId = new(data.GetByteArray("rid"));

            if (data.ContainsKey("lid"))
                doubleRef.LeftId = new(data.GetByteArray("lid"));

            return doubleRef;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            if (LeftId != default && RightId != default)
                tooltips.Add(new(Mod, "TIDRefIds", $"[c/aaaa00:Referenced IDs:] {World.Guids.GetShortGuid(LeftId)}, {World.Guids.GetShortGuid(RightId)}"));
            else if (LeftId != default)
                tooltips.Add(new(Mod, "TIDRefIds", $"[c/aaaa00:Referenced ID:] {World.Guids.GetShortGuid(LeftId)}"));
        }

        public abstract VariableValue GetValue(ComponentSystem system, VariableValue left, VariableValue right, List<Error> errors);
        public abstract ReturnType[] GetValidRightSlotTypes(ReturnType leftSlotType);

        public virtual DoubleReferenceVariable CreateVariable(Variable left, Variable right) => (DoubleReferenceVariable)Activator.CreateInstance(GetType());
    }
}
