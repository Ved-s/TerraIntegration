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
    public class Float : VariableValue, IToString
    {
        public override string Type => "float";
        public override string TypeDisplay => "Float";

        public override Color DisplayColor => Color.Green;

        public float Value { get; set; }

        public Float() { }
        public Float(float value) { Value = value; }

        public override string Display()
        {
            return Value.ToString();
        }

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
    }
}
