using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerraIntegration.Values;
using Terraria.ModLoader;

namespace TerraIntegration.Variables
{
    public class Constant : Variable
    {
        public override string Type => "const";
        public override string TypeDisplay => "Constant";

        public override string TypeDescription => $"[c/aaaa00:Value:] {Util.ColorTag(Value.DisplayColor, Value.Display())}";

        public VariableValue Value { get; set; } = new();

        public override Type VariableReturnType => Value.GetType();

        public Constant() { }

        public Constant(VariableValue value)
        {
            Value = value;
        }

        public override VariableValue GetValue(ComponentSystem system, HashSet<Error> errors)
        {
            return Value;
        }

        protected override void SaveCustomData(BinaryWriter writer)
        {
            Value.SaveData(writer);
        }

        protected override Variable LoadCustomData(BinaryReader reader)
        {
            return new Constant(VariableValue.LoadData(reader));
        }

        public override Variable GetFromCommand(CommandCaller caller, List<string> args)
        {
            if (args.Count < 1)
            {
                caller.Reply("Argument required: value type");
                return null;
            }

            if (!VariableValue.ByTypeName.TryGetValue(args[0], out VariableValue val))
            {
                caller.Reply($"Unregistered value type {args[0]}");
                return null;
            }
            args.RemoveAt(0);

            VariableValue res = val.GetFromCommand(caller, args);
            if (res is null)
            {
                return null;
            }

            return new Constant(res);
        }
    }
}
