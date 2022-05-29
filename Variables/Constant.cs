using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TerraIntegration.Values;
using Terraria.ModLoader;

namespace TerraIntegration.Variables
{
    public class Constant : Variable
    {
        public override string Type => "const";
        public override string TypeDisplay => "Constant";

        public VariableValue Value { get; set; } = new();

        public override Type VariableReturnType => Value.GetType();

        public override SpriteSheetPos SpriteSheetPos => new(BasicSheet, 2, 0);

        Regex LineBreaksWithPadding = new(@"\r?\n[ \t]*", RegexOptions.Compiled);

        public Constant() { }

        public Constant(VariableValue value)
        {
            Value = value;
        }

        public override VariableValue GetValue(ComponentSystem system, List<Error> errors)
        {
            return Value;
        }

        protected override void SaveCustomData(BinaryWriter writer)
        {
            Value.SaveData(writer);
        }

        protected override Variable LoadCustomData(BinaryReader reader)
        {
            return new Constant(VariableValue.LoadData(reader));
        }

        public override Variable GetFromCommand(CommandCaller caller, List<string> args)
        {
            if (args.Count < 1)
            {
                caller.Reply("Argument required: value type");
                return null;
            }

            if (!VariableValue.ByTypeName.TryGetValue(args[0], out VariableValue val))
            {
                caller.Reply($"Unregistered value type {args[0]}");
                return null;
            }
            args.RemoveAt(0);

            VariableValue res = val.GetFromCommand(caller, args);
            if (res is null)
            {
                return null;
            }

            return new Constant(res);
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            if (Value is not null)
            {
                string text = Value.Display(null).HoverText;
                text = LineBreaksWithPadding.Replace(text, " ");
                if (text is not null)
                    tooltips.Add(new(Mod, "TIConstantValue", $"[c/aaaa00:Value:] {text}"));
            }
        }
    }
}
