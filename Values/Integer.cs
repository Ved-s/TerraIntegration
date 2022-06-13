using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using TerraIntegration.Basic;
using TerraIntegration.DataStructures;
using TerraIntegration.DisplayedValues;
using TerraIntegration.Interfaces;
using TerraIntegration.Interfaces.Math;
using TerraIntegration.UI;
using TerraIntegration.Variables;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader;

namespace TerraIntegration.Values
{
    public class Integer : VariableValue, INumeric, IToString, IProgrammable
    {
        public override string TypeName => "int";
        public override string TypeDefaultDisplayName => "Integer";
        public override string TypeDefaultDescription => "32-bit signed integer";

        public override Color TypeColor => Color.Orange;

        public override SpriteSheetPos SpriteSheetPos => new(BasicSheet, 0, 0);

        public int Value { get; set; }

        public long NumericValue => Value;
        public long NumericMax => int.MaxValue;
        public long NumericMin => int.MinValue;

        public UIPanel Interface { get; set; }
        public bool HasComplexInterface => false;

        public byte BitWidth => 32;

        public UIFocusInputTextField InterfaceValue;
        static Regex NotDigit = new(@"\D+", RegexOptions.Compiled);

        public Integer() { }
        public Integer(int value) { Value = value; }

        public override DisplayedValue Display(ComponentSystem system) => new ColorTextDisplay(Value.ToString(), TypeColor);

        protected override VariableValue LoadCustomData(BinaryReader reader)
        {
            return new Integer(reader.ReadInt32());
        }

        protected override void SaveCustomData(BinaryWriter writer)
        {
            writer.Write(Value);
        }

        public override VariableValue GetFromCommand(CommandCaller caller, List<string> args)
        {
            if (args.Count < 1)
            {
                caller.Reply("Argument required: int value");
                return null;
            }

            if (!int.TryParse(args[0], out int val))
            {
                caller.Reply($"Value is not an integer: {args[0]}");
                return null;
            }
            args.RemoveAt(0);

            return new Integer(val);
        }

        public override string ToString()
        {
            return Value.ToString();
        }

        public override bool Equals(VariableValue obj)
        {
            return obj is Integer integer &&
                   TypeName == integer.TypeName &&
                   Value == integer.Value;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(TypeName, Value);
        }

        public VariableValue FromNumericChecked(long value, List<Error> errors)
        {
            return new Integer((int)value);
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

                    if (int.TryParse(@new, out _)) return @new;
                    return old;
                }
            };
            Interface.Append(InterfaceValue);
        }

        public Basic.Variable WriteVariable()
        {
            if (int.TryParse(InterfaceValue.CurrentString, out int value))
                return new Constant(new Integer(value));
            return null;
        }
    }
}
