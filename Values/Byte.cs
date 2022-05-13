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
    public class Byte : VariableValue, INumeric, IToString
    {
        public override string Type => "byte";
        public override string TypeDisplay => "Byte";

        public override Color TypeColor => Color.Blue;

        public byte Value { get; set; }
        public long NumericValue => Value;

        public Byte() { }
        public Byte(byte value) { Value = value; }

        public override string Display()
        {
            return Value.ToString();
        }

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
    }
}
