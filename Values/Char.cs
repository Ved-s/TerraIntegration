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
    [Autoload(false)]
    public class Char : VariableValue, IToString
    {
        public override string Type => "char";
        public override string TypeDisplay => "Char";

        public override Color TypeColor => Color.LightPink;

        public override SpriteSheet SpriteSheet => BasicSheet;
        public override Point SpritesheetPos => new(3, 0);

        public char Value { get; set; }

        public Char() { }
        public Char(char value) { Value = value; }

        public override string Display()
        {
            return Value.ToString();
        }

        protected override VariableValue LoadCustomData(BinaryReader reader)
        {
            return new Char(reader.ReadChar());
        }

        protected override void SaveCustomData(BinaryWriter writer)
        {
            writer.Write(Value);
        }

        public override VariableValue GetFromCommand(CommandCaller caller, List<string> args)
        {
            if (args.Count < 1)
            {
                caller.Reply("Argument required: char value");
                return null;
            }

            if (!char.TryParse(args[0], out char val))
            {
                caller.Reply($"Value is not a char: {args[0]}");
                return null;
            }
            args.RemoveAt(0);

            return new Char(val);
        }

        public override string ToString()
        {
            return Value.ToString();
        }

        public override bool Equals(object obj)
        {
            return obj is Char @char &&
                   Type == @char.Type &&
                   Value == @char.Value;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Type, Value);
        }
    }
}
