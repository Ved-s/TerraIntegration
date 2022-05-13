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
    public class Boolean : VariableValue, IToString
    {
        public override string Type => "bool";
        public override string TypeDisplay => "Boolean";

        public override Color TypeColor => Color.CadetBlue;

        public bool Value { get; set; }

        public Boolean() { }
        public Boolean(bool value) { Value = value; }

        private static HashSet<string> TrueValues = new() { "true", "1", "yes", "t" };
        private static HashSet<string> FalseValues = new() { "false", "0", "no", "f" };

        public override string Display()
        {
            return Value.ToString();
        }

        protected override VariableValue LoadCustomData(BinaryReader reader)
        {
            return new Boolean(reader.ReadBoolean());
        }

        protected override void SaveCustomData(BinaryWriter writer)
        {
            writer.Write(Value);
        }

        public override VariableValue GetFromCommand(CommandCaller caller, List<string> args)
        {
            if (args.Count < 1)
            {
                caller.Reply("Argument required: true/false value");
                return null;
            }

            bool v;
            string arg = args[0].ToLower();

            if (TrueValues.Contains(arg)) v = true;
            else if (FalseValues.Contains(arg)) v = false;
            else 
            {
                caller.Reply($"Value is not a boolean: {args[0]}");
                return null;
            }
            args.RemoveAt(0);

            return new Boolean(v);
        }

        public override string ToString()
        {
            return Value.ToString();
        }

        public override bool Equals(object obj)
        {
            return obj is Boolean boolean &&
                   Type == boolean.Type &&
                   Value == boolean.Value;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Type, Value);
        }
    }
}
