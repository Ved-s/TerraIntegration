using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TerraIntegration.Interfaces;
using TerraIntegration.Interfaces.Math;
using TerraIntegration.UI;
using TerraIntegration.Values;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader;

namespace TerraIntegration.Variables.Numeric
{
    public class Add : DoubleReferenceVariable
    {
        public override string Type => "add";
        public override string TypeDisplay => "Add";

        public override SpriteSheetPos SpriteSheetPos => new(MathSheet, 0, 0);

        public override Type[] LeftSlotValueTypes => new[] { typeof(IAddable) };

        public override UIDrawing CenterDrawing => new UIDrawing()
        {
            OnDraw = (e, sb, style) =>
            {
                VariableRenderer.DrawVariableOverlay(sb, false, null, Type, style.Position() - new Vector2(16), new(32), Color.White, 0f, Vector2.Zero);
            }
        };

        public Add() { }
        public Add(Guid left, Guid right)
        {
            LeftId = left;
            RightId = right;
        }
        public override Type[] GetValidRightSlotTypes(Type leftSlotType)
        {
            if (VariableValue.ByType.TryGetValue(leftSlotType, out VariableValue value) && value is IAddable addable)
                return addable.ValidAddTypes;
            return null;
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
        public override VariableValue GetValue(ComponentSystem system, VariableValue left, VariableValue right, List<Error> errors)
        {
            IAddable addable = (IAddable)left;

            VariableValue result = addable.Add(right, errors);

            if (result is not null)
                SetReturnTypeCache(result.GetType());
            return result;
        }
        public override DoubleReferenceVariable CreateVariable(Variable left, Variable right)
        {
            return new Add()
            {
                VariableReturnType = left.VariableReturnType
            };
        }
    }
}
