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
    public class Double : VariableValue, IToString
    {
        public override string Type => "double";
        public override string TypeDisplay => "Double";

        public override Color DisplayColor => Color.Lime;

        public double Value { get; set; }

        public Double() { }
        public Double(double value) { Value = value; }

        public override string Display()
        {
            return Value.ToString();
        }

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
    }
}
