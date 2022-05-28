using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TerraIntegration.DataStructures;
using TerraIntegration.DisplayedValues;
using TerraIntegration.Interfaces;
using TerraIntegration.Items;
using TerraIntegration.UI;
using TerraIntegration.Variables;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader;

namespace TerraIntegration.Values
{
    public class Double : VariableValue, IToString, IDecimal, IOwnProgrammerInterface
    {
        public override string Type => "double";
        public override string TypeDisplay => "Double";

        public override SpriteSheetPos SpriteSheetPos => new(BasicSheet, 0, 1);

        public override Color TypeColor => Color.Lime;

        public double Value { get; set; }
        public UIPanel Interface { get; set; }

        public double DecimalValue => Value;
        public double DecimalMax => double.MaxValue;
        public double DecimalMin => double.MinValue;

        public UIFocusInputTextField InterfaceValue;
        static Regex NotDigitOrDot = new(@"[^\d\.]+", RegexOptions.Compiled);

        public Double() { }
        public Double(double value) { Value = value; }

        public override DisplayedValue Display() => new ColorTextDisplay(Value.ToString(), TypeColor);

        protected override VariableValue LoadCustomData(BinaryReader reader)
        {
            return new Double(reader.ReadDouble());
        }

        protected override void SaveCustomData(BinaryWriter writer)
        {
            writer.Write(Value);
        }

        public override VariableValue GetFromCommand(CommandCaller caller, List<string> args)
        {
            if (args.Count < 1)
            {
                caller.Reply("Argument required: double value");
                return null;
            }

            if (!double.TryParse(args[0], out double val))
            {
                caller.Reply($"Value is not a double: {args[0]}");
                return null;
            }
            args.RemoveAt(0);

            return new Double(val);
        }

        public override string ToString()
        {
            return Value.ToString();
        }

        public override bool Equals(object obj)
        {
            return obj is Double @double &&
                   Type == @double.Type &&
                   Value == @double.Value;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Type, Value);
        }

        public void SetupInterface()
        {
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
                    if (double.TryParse(@new, out _)) return @new;
                    return old;
                }
            };
            Interface.Append(InterfaceValue);
        }

        public Variables.Variable WriteVariable()
        {
            if (double.TryParse(InterfaceValue.CurrentString, out double value))
                return new Constant(new Double(value));
            return null;
        }

        public VariableValue FromDecimalChecked(double value, List<Error> errors)
        {
            return new Double(value);
        }
    }
}
