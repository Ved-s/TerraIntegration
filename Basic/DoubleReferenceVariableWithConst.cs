﻿using Microsoft.Xna.Framework;
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

namespace TerraIntegration.Basic
{
    public abstract class DoubleReferenceVariableWithConst : Variable, IOwnProgrammerInterface
    {
        public UIPanel Interface { get; set; }

        public UIVariableSlot LeftSlot { get; set; }
        public UIConstantOrReference RightSlot { get; set; }

        public abstract Type[] LeftSlotValueTypes { get; }
        public virtual UIDrawing CenterDrawing => new UIDrawing()
        {
            OnDraw = (e, sb, style) =>
            {
                VariableRenderer.DrawVariableOverlay(sb, false, null, Type, style.Position() - new Vector2(16), new(32), Color.White, 0f, Vector2.Zero);
            }
        };

        public Guid Left { get; set; }
        public ValueOrRef Right { get; set; }
        public bool HasComplexInterface => true;

        private Type[] ValidRightTypes;
        private Dictionary<Type, Type[]> ValidTypesCache = new();
        private HashSet<(Type, Type)> ValidTypePairs = new();
        
        public void SetupInterface()
        {
            Interface.Append(LeftSlot = new()
            {
                Top = new(-21, .5f),
                Left = new(-75, .5f),

                DisplayOnly = true,
                VariableValidator = (var) => LeftSlotValueTypes is not null && LeftSlotValueTypes.Any(t => t.IsAssignableFrom(var.VariableReturnType)),
                HoverText = LeftSlotValueTypes is null ? null : string.Join(", ", LeftSlotValueTypes.Select(t => VariableValue.TypeToName(t, true))),

                VariableChanged = (var) =>
                {
                    if (var?.Var.VariableReturnType is null)
                    {
                        ValidRightTypes = null;
                        RightSlot.ValidTypes = null;
                        return;
                    }

                    ValidRightTypes = GetValidRightSlotTypes(var.Var.VariableReturnType);
                    if (ValidRightTypes is null)
                    {
                        ValidRightTypes = null;
                        RightSlot.ValidTypes = null;
                        return;
                    }

                    RightSlot.ValidTypes = ValidRightTypes;
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
                Top = new(-59, .5f),
                Left = new(30, .5f),
                BackgroundColor = Color.Transparent,
                BorderColor = Color.Transparent,
            });
        }

        public Variable WriteVariable()
        {
            ValueOrRef rightValue = RightSlot?.GetValue();

            if (LeftSlot?.Var?.Var is null || rightValue is null) return null;

            DoubleReferenceVariableWithConst doubleRef = CreateVariable(LeftSlot.Var.Var, rightValue);

            if (doubleRef is null) return null;

            doubleRef.Left = LeftSlot.Var.Var.Id;
            doubleRef.Right = rightValue;

            return doubleRef;
        }

        public override VariableValue GetValue(ComponentSystem system, List<Error> errors)
        {
            VariableValue left = system.GetVariableValue(Left, errors);
            VariableValue right = Right.GetValue(system, errors);

            if (left is null || right is null) return null;
            Type leftType = left.GetType();
            Type rightType = right.GetType();

            if (!ValidTypePairs.Contains((leftType, rightType)))
            {
                if (LeftSlotValueTypes is not null && !LeftSlotValueTypes.Any(t => t.IsAssignableFrom(leftType)))
                {
                    errors.Add(new(
                        ErrorType.ExpectedValuesWithId,
                        string.Join(", ", LeftSlotValueTypes.Select(t => VariableValue.TypeToName(t, true))),
                        World.Guids.GetShortGuid(Id)));
                    return null;
                }

                if (!ValidTypesCache.TryGetValue(leftType, out Type[] validRightTypes))
                {
                    validRightTypes = GetValidRightSlotTypes(leftType);
                    ValidTypesCache[leftType] = validRightTypes;
                }

                if (validRightTypes is not null && !validRightTypes.Any(t => t.IsAssignableFrom(rightType)))
                {
                    errors.Add(new(
                        ErrorType.ExpectedValuesWithId,
                        string.Join(", ", validRightTypes.Select(t => VariableValue.TypeToName(t, true))),
                        World.Guids.GetShortGuid(Id)));
                    return null;
                }

                ValidTypePairs.Add((leftType, rightType));
            }

            return GetValue(system, left, right, errors);
        }
        protected override void SaveCustomData(BinaryWriter writer)
        {
            writer.Write(Left.ToByteArray());
            Right.SaveData(writer);
        }
        protected override Variable LoadCustomData(BinaryReader reader)
        {
            var doubleRef = (DoubleReferenceVariableWithConst)Activator.CreateInstance(GetType());

            doubleRef.Left = new(reader.ReadBytes(16));
            doubleRef.Right = ValueOrRef.LoadData(reader);

            return doubleRef;
        }
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            if (Left != default && Right is not null)
                tooltips.Add(new(Mod, "TIDVars", $"[c/aaaa00:Values:] Ref {World.Guids.GetShortGuid(Left)}, {Right}"));
        }

        public abstract VariableValue GetValue(ComponentSystem system, VariableValue left, VariableValue right, List<Error> errors);
        public abstract Type[] GetValidRightSlotTypes(Type leftSlotType);

        public virtual DoubleReferenceVariableWithConst CreateVariable(Variable left, ValueOrRef right) 
            => (DoubleReferenceVariableWithConst)Activator.CreateInstance(GetType());
    }
}