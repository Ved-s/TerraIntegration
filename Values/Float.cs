using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TerraIntegration.DisplayedValues;
using TerraIntegration.Interfaces;
using TerraIntegration.UI;
using TerraIntegration.Variables;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader;

namespace TerraIntegration.Values
{
    public class Float : VariableValue, IToString, IDecimal, IOwnProgrammerInterface
    {
        public override string Type => "float";
        public override string TypeDisplay => "Float";

        public override Color TypeColor => Color.Green;

        public override SpriteSheet SpriteSheet => BasicSheet;
        public override Point SpritesheetPos => new(1, 1);

        public float Value { get; set; }
        public UIPanel Interface { get; set; }

        public double DecimalValue => Value;
        public double DecimalMax => float.MaxValue;
        public double DecimalMin => float.MinValue;

        public UIFocusInputTextField InterfaceValue;
        static Regex NotDigitOrDot = new(@"[^\d\.]+", RegexOptions.Compiled);

        public Float() { }
        public Float(float value) { Value = value; }

        public override DisplayedValue Display() => new ColorTextDisplay(Value.ToString(), TypeColor);

        protected override VariableValue LoadCustomData(BinaryReader reader)
        {
            return new Float(reader.ReadSingle());
        }

        protected override void SaveCustomData(BinaryWriter writer)
        {
            writer.Write(Value);
        }

        public override VariableValue GetFromCommand(CommandCaller caller, List<string> args)
        {
            if (args.Count < 1)
            {
                caller.Reply("Argument required: float value");
                return null;
            }

            if (!float.TryParse(args[0], out float val))
            {
                caller.Reply($"Value is not a float: {args[0]}");
                return null;
            }
            args.RemoveAt(0);

            return new Float(val);
        }

        public override string ToString()
        {
            return Value.ToString();
        }

        public override bool Equals(object obj)
        {
            return obj is Float @float &&
                   Type == @float.Type &&
                   Value == @float.Value;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Type, Value);
        }

        public void SetupInterface()
        {
            UIPanel p = new();

            InterfaceValue = new("")
            {
                Top = new(-12, .5f),
                Left = new(20, 0),
                Width = new(-40, 1),
                Height = new(25, 0),
                ModifyTextInput = (@new, old) =>
                {
                    if (@new.IsNullEmptyOrWhitespace()) return "";
                    bool neg = @new.Count(c => c == '-') % 2 == 1;
                    @new = NotDigitOrDot.Replace(@new, "");
                    if (neg) @new = '-' + @new;
                    if (float.TryParse(@new, out _)) return @new;
                    return old;
                }
            };
            p.Append(InterfaceValue);
            Interface = p;
        }

        public void WriteVariable(Items.Variable var)
        {
            if (float.TryParse(InterfaceValue.CurrentString, out float value))
                var.Var = new Constant(new Float(value));
        }

        public VariableValue FromDecimalChecked(double value, List<Error> errors)
        {
            return new Float((float)value);
        }
    }
}
