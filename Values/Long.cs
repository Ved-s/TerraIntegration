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
    public class Long : VariableValue, INumeric, IToString, IProgrammable
    {
        public override string TypeName => "long";
        public override string TypeDefaultDisplayName => "Long";
        public override string TypeDefaultDescription => "64-bit signed integer";

        public override Color TypeColor => Color.Red;

        public override SpriteSheetPos SpriteSheetPos => new(BasicSheet, 1, 2);

        public long Value { get; set; }

        public long NumericValue => Value;
        public long NumericMax => long.MaxValue;
        public long NumericMin => long.MinValue;

        public UIPanel Interface { get; set; }
        public bool HasComplexInterface => false;

        public byte BitWidth => 64;

        public UIFocusInputTextField InterfaceValue;
        static Regex NotDigit = new(@"\D+", RegexOptions.Compiled);

        public Long() { }
        public Long(long value) { Value = value; }

        public override DisplayedValue Display(ComponentSystem system) => new ColorTextDisplay(Value.ToString(), TypeColor);

        protected override VariableValue LoadCustomData(BinaryReader reader)
        {
            return new Long(reader.ReadInt64());
        }

        protected override void SaveCustomData(BinaryWriter writer)
        {
            writer.Write(Value);
        }

        public override string ToString()
        {
            return Value.ToString();
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(TypeName, Value);
        }

        public VariableValue FromNumericChecked(long value, List<Error> errors)
        {
            return new Long(value);
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

                    if (long.TryParse(@new, out _)) return @new;
                    return old;
                }
            };
            Interface.Append(InterfaceValue);
        }

        public Basic.Variable WriteVariable()
        {
            if (long.TryParse(InterfaceValue.CurrentString, out long value))
                return new Constant(new Long(value));
            return null;
        }

        public override bool Equals(VariableValue obj)
        {
            return obj is Long @long &&
                   TypeName == @long.TypeName &&
                   Value == @long.Value;
        }
    }
}
