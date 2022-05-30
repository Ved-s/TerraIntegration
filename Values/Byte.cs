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
using TerraIntegration.Items;
using TerraIntegration.UI;
using TerraIntegration.Variables;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader;

namespace TerraIntegration.Values
{
    public class Byte : VariableValue, INumeric, IToString, IOwnProgrammerInterface
    {
        public override string Type => "byte";
        public override string TypeDisplay => "Byte";

        public override Color TypeColor => Color.Blue;

        public override SpriteSheetPos SpriteSheetPos => new(BasicSheet, 2, 0);

        public byte Value { get; set; }

        public long NumericValue => Value;
        public long NumericMax => byte.MaxValue;
        public long NumericMin => byte.MinValue;

        public UIPanel Interface { get; set; }
        public UIFocusInputTextField InterfaceValue;
        static Regex NotDigit = new(@"\D+", RegexOptions.Compiled);

        public Byte() { }
        public Byte(byte value) { Value = value; }

        public override DisplayedValue Display(ComponentSystem system) => new ColorTextDisplay(Value.ToString(), TypeColor);

        protected override VariableValue LoadCustomData(BinaryReader reader)
        {
            return new Byte(reader.ReadByte());
        }

        protected override void SaveCustomData(BinaryWriter writer)
        {
            writer.Write(Value);
        }

        public override VariableValue GetFromCommand(CommandCaller caller, List<string> args)
        {
            if (args.Count < 1)
            {
                caller.Reply("Argument required: byte value");
                return null;
            }

            if (!byte.TryParse(args[0], out byte val))
            {
                caller.Reply($"Value is not a byte: {args[0]}");
                return null;
            }
            args.RemoveAt(0);

            return new Byte(val);
        }

        public override string ToString()
        {
            return Value.ToString();
        }

        public override bool Equals(object obj)
        {
            return obj is Byte @byte &&
                   Type == @byte.Type &&
                   Value == @byte.Value;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Type, Value);
        }

        public VariableValue FromNumericChecked(long value, List<Error> errors)
        {
            return new Byte((byte)value);
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
                    @new = NotDigit.Replace(@new, "");
                    if (byte.TryParse(@new, out _)) return @new;
                    return old;
                }
            };
            Interface.Append(InterfaceValue);
        }

        public Basic.Variable WriteVariable()
        {
            if (byte.TryParse(InterfaceValue.CurrentString, out byte value))
                return new Constant(new Byte(value));
            return null;
        }
    }
}
