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
    public class String : VariableValue, IToString, IAddable
    {
        public override string Type => "str";
        public override string TypeDisplay => "String";

        public override Color TypeColor => Color.OrangeRed;

        public string Value { get; set; }
        public Type[] ValidAddTypes => new[] { typeof(IToString) };

        public String() { }
        public String(string value) { Value = value; }

        public override string Display()
        {
            return Value;
        }

        protected override VariableValue LoadCustomData(BinaryReader reader)
        {
            return new String(reader.ReadString());
        }

        protected override void SaveCustomData(BinaryWriter writer)
        {
            writer.Write(Value);
        }

        public override VariableValue GetFromCommand(CommandCaller caller, List<string> args)
        {
            if (args.Count < 1)
            {
                caller.Reply("Argument required: string value");
                return null;
            }

            string arg = args[0];
            args.RemoveAt(0);

            return new String(arg);
        }

        public override string ToString()
        {
            return Value;
        }

        public VariableValue Add(VariableValue value, List<Error> errors)
        {
            if (value is IToString toString)
                return new String(Value + toString.ToString());
            
            errors.Add(new(ErrorType.ExpectedValue, TypeToName(typeof(IToString), out _)));
            return null;
        }

        public override bool Equals(object obj)
        {
            return obj is String @string &&
                   Type == @string.Type &&
                   Value == @string.Value;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Type, Value);
        }
    }
}
