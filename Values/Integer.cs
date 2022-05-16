using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerraIntegration.Interfaces;
using Terraria.ModLoader;

namespace TerraIntegration.Values
{
    public class Integer : VariableValue, IAddable, INumeric, IToString
    {
        public override string Type => "int";
        public override string TypeDisplay => "Integer";

        public override Color TypeColor => Color.Orange;

        public override SpriteSheet SpriteSheet => BasicSheet;
        public override Point SpritesheetPos => new(0, 0);

        public int Value { get; set; }
        public long NumericValue => Value;

        public Type[] ValidAddTypes => new[] { typeof(INumeric) };

        public Integer() { }
        public Integer(int value) { Value = value; }

        public override string Display()
        {
            return Value.ToString();
        }

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

        public VariableValue Add(VariableValue value, List<Error> errors)
        {
            if (value is INumeric numeric)
            {
                long num = numeric.NumericValue;
                if (num > int.MaxValue)
                {
                    errors.Add(new(ErrorType.ValueTooBigForType, num, TypeDisplay));
                    return null;
                }
                if (num < int.MinValue)
                {
                    errors.Add(new(ErrorType.ValueTooSmallForType, num, TypeDisplay));
                    return null;
                }
                return new Integer(Value + (int)num);
            }

            errors.Add(new(ErrorType.ExpectedValue, TypeToName(typeof(INumeric), out _)));
            return null;
        }

        public override string ToString()
        {
            return Value.ToString();
        }

        public override bool Equals(object obj)
        {
            return obj is Integer integer &&
                   Type == integer.Type &&
                   Value == integer.Value;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Type, Value);
        }
    }
}
