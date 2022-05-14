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
    public class EventSubscriber : Variable
    {
        public override string Type => "eventsub";
        public override string TypeDisplay => "Event Subscriber";

        public override Type VariableReturnType => typeof(Values.Boolean);

        public Guid EventId { get; set; }
        public bool Triggered { get; set; }

        public EventSubscriber() { }
        public EventSubscriber(Guid eventId) 
        {
            EventId = eventId;
        }

        public override VariableValue GetValue(ComponentSystem system, List<Error> errors)
        {
            VariableValue v = new Values.Boolean(Triggered);
            Triggered = false;
            return v;
        }

        protected override void SaveCustomData(BinaryWriter writer)
        {
            writer.Write(EventId.ToByteArray());
        }

        protected override Variable LoadCustomData(BinaryReader reader)
        {
            return new EventSubscriber(new Guid(reader.ReadBytes(16)));
        }

        public override Variable GetFromCommand(CommandCaller caller, List<string> args)
        {
            if (args.Count < 1)
            {
                caller.Reply("Argument required: event variable id");
                return null;
            }

            Guid? id = World.Guids.GetGuid(args[0]);

            if (id is null)
            {
                caller.Reply($"Id not found: {args[0]}");
                return null;
            }
            args.RemoveAt(0);

            return new EventSubscriber(id.Value);
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            if (EventId != default)
                tooltips.Add(new(Mod, "TIEventId", $"[c/aaaa00:Event ID:] {World.Guids.GetShortGuid(EventId)}"));
        }
    }
}
