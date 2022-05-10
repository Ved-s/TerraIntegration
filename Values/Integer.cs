using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;

namespace TerraIntegration.Values
{
    public class Integer : VariableValue
    {
        public override string Type => "int";
        public override string TypeDisplay => "Integer";

        public override Color DisplayColor => Color.Orange;

        public int Value { get; set; }

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
    }
}
