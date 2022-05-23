using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TerraIntegration.Interfaces;
using TerraIntegration.UI;
using TerraIntegration.Values;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader;

namespace TerraIntegration.Variables
{
    public class Add : Variable, IOwnProgrammerInterface
    {
        public override string Type => "add";
        public override string TypeDisplay => "Add";

        public Guid First { get; set; }
        public Guid Second { get; set; }

        public override SpriteSheet SpriteSheet => MathSheet;
        public override Point SpritesheetPos => new(0, 0);

        public UIPanel Interface { get; set; }
        public UIVariableSlot SlotA, SlotB;
        public Type[] ValidAddTypes;

        public Add() { }
        public Add(Guid first, Guid second)
        {
            First = first;
            Second = second;
        }

        protected override void SaveCustomData(BinaryWriter writer)
        {
            writer.Write(First.ToByteArray());
            writer.Write(Second.ToByteArray());
        }

        protected override Variable LoadCustomData(BinaryReader reader)
        {
            return new Add(new Guid(reader.ReadBytes(16)), new Guid(reader.ReadBytes(16)));
        }

        public override VariableValue GetValue(ComponentSystem system, List<Error> errors)
        {
            VariableValue first = system.GetVariableValue(First, errors);
            VariableValue second = system.GetVariableValue(Second, errors);

            if (first is null || second is null) return null;

            if (first is not IAddable addable)
            {
                errors.Add(new(ErrorType.ExpectedValueWithId, "Addable", World.Guids.GetShortGuid(Id)));
                return null;
            }
            bool anyMatch = false;
            Type secondType = second.GetType();
            foreach (Type t in addable.ValidAddTypes)
                if (secondType.IsAssignableTo(t))
                {
                    anyMatch = true;
                    break;
                }
            if (!anyMatch)
            {
                IEnumerable<string> strings = addable.ValidAddTypes.Select(t => VariableValue.TypeToName(t, out _));
                errors.Add(new(ErrorType.ExpectedValuesWithId, string.Join(", ", strings), World.Guids.GetShortGuid(Id)));
                return null;
            }

            VariableValue result = addable.Add(second, errors);
            SetReturnTypeCache(result.GetType());
            return result;
        }

        public override Variable GetFromCommand(CommandCaller caller, List<string> args)
        {
            if (args.Count < 1)
            {
                caller.Reply("Argument required: first variable id");
                return null;
            }
            if (args.Count < 2)
            {
                caller.Reply("Argument required: second variable id");
                return null;
            }

            Guid? first = World.Guids.GetGuid(args[0]);
            Guid? second = World.Guids.GetGuid(args[1]);

            if (first is null)
            {
                caller.Reply($"First id not found: {args[0]}");
                return null;
            }
            if (second is null)
            {
                caller.Reply($"Second id not found: {args[1]}");
                return null;
            }
            args.RemoveAt(0);
            args.RemoveAt(0);

            return new Add(first.Value, second.Value);
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            if (First != default && Second != default)
                tooltips.Add(new(Mod, "TIAddIds", $"[c/aaaa00:Referenced IDs:] {World.Guids.GetShortGuid(First)}, {World.Guids.GetShortGuid(Second)}"));
        }

        public void SetupInterface()
        {
            UIPanel panel = new UIPanel();
            Interface = panel;

            panel.Append(SlotA = new()
            {
                Top = new(-21, .5f),
                Left = new(-75, .5f),

                DisplayOnly = true,
                VariableValidator = (var) => typeof(IAddable).IsAssignableFrom(var.VariableReturnType),
                HoverText = VariableValue.TypeToColorTagName(typeof(IAddable)),

                VariableChanged = (var) =>
                {
                    if (var?.Var.VariableReturnType is not null
                    && VariableValue.ByType.TryGetValue(var.Var.VariableReturnType, out VariableValue val)
                    && val is IAddable addable)
                    {
                        ValidAddTypes = addable.ValidAddTypes;
                        SlotB.HoverText = string.Join(", ", ValidAddTypes.Select(VariableValue.TypeToColorTagName));
                    }
                    else
                    {
                        ValidAddTypes = null;
                        SlotB.HoverText = null;
                    }
                }
            });

            panel.Append(new UIDrawing()
            {
                Top = new(-15, .5f),
                Left = new(-15, .5f),

                Width = new(30, 0),
                Height = new(30, 0),

                OnDraw = (e, sb, style) =>
                {
                    sb.DrawLine(style.Position() + new Vector2(16, 0), (float)Math.PI * 0.5f, 30, Color.White, 4);
                    sb.DrawLine(style.Position() + new Vector2(-1, 13), 0, 30, Color.White, 4);
                }
            });

            panel.Append(SlotB = new()
            {
                Top = new(-21, .5f),
                Left = new(30, .5f),

                DisplayOnly = true,
                VariableValidator = (var) => ValidAddTypes is not null && ValidAddTypes.Any(t => t.IsAssignableFrom(var.VariableReturnType)),
            });
        }

        public void WriteVariable(Items.Variable var)
        {
            if (SlotA?.Var is null || SlotB?.Var is null) return;

            var.Var = new Add(SlotA.Var.Var.Id, SlotB.Var.Var.Id)
            {
                VariableReturnType = SlotA.Var.Var.VariableReturnType
            };
        }
    }
}
