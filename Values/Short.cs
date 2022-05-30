using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TerraIntegration.Basic;
using TerraIntegration.DisplayedValues;
using TerraIntegration.Interfaces;
using TerraIntegration.UI;
using TerraIntegration.Variables;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader;

namespace TerraIntegration.Values
{
    public class Short : VariableValue, INumeric, IToString, IOwnProgrammerInterface
    {
        public override string Type => "short";
        public override string TypeDisplay => "Short";

        public override Color TypeColor => Color.BlueViolet;

        public override SpriteSheetPos SpriteSheetPos => new(BasicSheet, 3, 1);

        public short Value { get; set; }

        public long NumericValue => Value;
        public long NumericMax => short.MaxValue;
        public long NumericMin => short.MinValue;

        public UIPanel Interface { get; set; }
        public UIFocusInputTextField InterfaceValue;
        static Regex NotDigit = new(@"\D+", RegexOptions.Compiled);

        public Short() { }
        public Short(short value) { Value = value; }

        public override DisplayedValue Display(ComponentSystem system) => new ColorTextDisplay(Value.ToString(), TypeColor);

        protected override VariableValue LoadCustomData(BinaryReader reader)
        {
            return new Short(reader.ReadInt16());
        }

        protected override void SaveCustomData(BinaryWriter writer)
        {
            writer.Write(Value);
        }

        public override VariableValue GetFromCommand(CommandCaller caller, List<string> args)
        {
            if (args.Count < 1)
            {
                caller.Reply("Argument required: short value");
                return null;
            }

            if (!short.TryParse(args[0], out short val))
            {
                caller.Reply($"Value is not a short: {args[0]}");
                return null;
            }
            args.RemoveAt(0);

            return new Short(val);
        }

        public override string ToString()
        {
            return Value.ToString();
        }

        public override bool Equals(object obj)
        {
            return obj is Short @byte &&
                   Type == @byte.Type &&
                   Value == @byte.Value;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Type, Value);
        }

        public VariableValue FromNumericChecked(long value, List<Error> errors)
        {
            return new Short((byte)value);
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
                    @new = NotDigit.Replace(@new, "");
                    if (neg) @new = '-' + @new;
                    if (short.TryParse(@new, out _)) return @new;
                    return old;
                }
            };
            Interface.Append(InterfaceValue);
        }

        public Basic.Variable WriteVariable()
        {
            if (short.TryParse(InterfaceValue.CurrentString, out short value))
                return new Constant(new Short(value));
            return null;
        }
    }
}
