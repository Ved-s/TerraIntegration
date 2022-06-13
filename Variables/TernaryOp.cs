using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerraIntegration.Basic;
using TerraIntegration.DataStructures;
using TerraIntegration.UI;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader;

namespace TerraIntegration.Variables
{
    public class TernaryOp : Variable, IProgrammable
    {
        public override string TypeName => "tern";
        public override string TypeDefaultDisplayName => "Ternary operator";
        public override string TypeDefaultDescription => "Returns one of two values based\non the condition value";

        public override SpriteSheetPos SpriteSheetPos => new(BasicSheet, 3, 2);

        protected override VariableMatch InitRelated => VariableMatch.OfReturnType<Values.Boolean>();

        public Guid Condition, TrueValue, FalseValue;

        public UIPanel Interface { get; set; }
        public bool HasComplexInterface => false;

        public UIVariableSlot ConditionSlot, TrueSlot, FalseSlot;

        public TernaryOp() { }
        public TernaryOp(Guid condition, Guid @true, Guid @false)
        {
            Condition = condition;
            TrueValue = @true;
            FalseValue = @false;
        }

        public override VariableValue GetValue(ComponentSystem system, List<Error> errors)
        {
            if (!TryGetValueType<Values.Boolean>(system.GetVariableValue(Condition, errors), errors, out var condition)) 
                return null;

            return condition.Value ? system.GetVariableValue(TrueValue, errors) : system.GetVariableValue(FalseValue, errors);
        }
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            if (Condition != default && TrueValue != default && FalseValue != default)
            {
                tooltips.Add(new(Mod, "TITernIds",
                    $"[c/aaaa00:Referenced IDs:] " +
                    $"{World.Guids.GetShortGuid(Condition)}, " +
                    $"{World.Guids.GetShortGuid(TrueValue)}, " +
                    $"{World.Guids.GetShortGuid(FalseValue)}"));
            }
        }

        protected override void SaveCustomData(BinaryWriter writer)
        {
            writer.Write(Condition.ToByteArray());
            writer.Write(TrueValue.ToByteArray());
            writer.Write(FalseValue.ToByteArray());
        }
        protected override Variable LoadCustomData(BinaryReader reader)
        {
            return new TernaryOp(new(reader.ReadBytes(16)), new(reader.ReadBytes(16)), new(reader.ReadBytes(16)));
        }

        public void SetupInterface()
        {
            Interface.Append(ConditionSlot = new()
            {
                DisplayOnly = true,
                Top = new(-21, .5f),
                Left = new(-101, .5f),

                VariableValidator = (var) => var.VariableReturnType == typeof(Values.Boolean),
                HoverText = $"A {VariableValue.TypeToName<Values.Boolean>()} condition value"
            });

            Interface.Append(new UITextPanel("?") 
            {
                Height = new(48, 0),
                Width = new(48, 0),

                Top = new(-24, .5f),
                Left = new(-63, .5f),

                BackgroundColor = Color.Transparent,
                BorderColor = Color.Transparent,

                PaddingTop = 0,
                PaddingBottom = 0,
            });

            Interface.Append(TrueSlot = new()
            {
                DisplayOnly = true,
                Top = new(-21, .5f),
                Left = new(-21, .5f),

                VariableValidator = (var) => FalseSlot.Var is null || var.VariableReturnType == FalseSlot.Var.Var.VariableReturnType,
                HoverText = "Value, which is returned if condition is True\nShould be the same type as False value"
            });

            Interface.Append(new UITextPanel(":")
            {
                Height = new(48, 0),
                Width = new(48, 0),

                Top = new(-24, .5f),
                Left = new(16, .5f),

                BackgroundColor = Color.Transparent,
                BorderColor = Color.Transparent,

                PaddingTop = 0,
                PaddingBottom = 0,
            });

            Interface.Append(FalseSlot = new()
            {
                DisplayOnly = true,
                Top = new(-21, .5f),
                Left = new(60, .5f),

                VariableValidator = (var) => TrueSlot.Var is null || var.VariableReturnType == TrueSlot.Var.Var.VariableReturnType,
                HoverText = "Value, which is returned if condition is False\nShould be the same type as True value"
            });
        }

        public Variable WriteVariable()
        {
            if (ConditionSlot.Var is null || TrueSlot.Var is null || FalseSlot.Var is null) 
                return null;

            return new TernaryOp(ConditionSlot.Var.Var.Id, TrueSlot.Var.Var.Id, FalseSlot.Var.Var.Id)
            {
                VariableReturnType = TrueSlot.Var.Var.VariableReturnType
            };
        }

        
    }
}
