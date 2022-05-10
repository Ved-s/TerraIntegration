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
    public class Reference : Variable
    {
        public override string Type => "ref";
        public override string TypeDisplay => "Reference";

        public override string TypeDescription => RefId == default ? null : $"[c/aaaa00:Referenced ID:] {World.Guids.GetShortGuid(RefId)}";

        public override Type VariableReturnType => returnType;

        public Guid RefId { get; set; }

        private Type returnType = typeof(VariableValue);

        public Reference() { }
        public Reference(Guid refId)
        {
            RefId = refId;
        }

        public override VariableValue GetValue(ComponentSystem system, HashSet<Error> errors)
        {
            VariableValue val = system.GetVariableValue(RefId, errors);
            if (val is not null && errors.Count == 0)
                returnType = val.GetType();
            return val;
        }

        protected override void SaveCustomData(BinaryWriter writer)
        {
            writer.Write(RefId.ToByteArray());
        }

        protected override Variable LoadCustomData(BinaryReader reader)
        {
            return new Reference(new Guid(reader.ReadBytes(16)));
        }

        public override Variable GetFromCommand(CommandCaller caller, List<string> args)
        {
            if (args.Count < 1)
            {
                caller.Reply("Argument required: variable id");
                return null;
            }

            Guid? id = World.Guids.GetGuid(args[0]);

            if (id is null)
            {
                caller.Reply($"Id not found: {args[0]}");
                return null;
            }
            args.RemoveAt(0);

            return new Reference(id.Value);
        }
    }
}
