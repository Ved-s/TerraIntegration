using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerraIntegration.Basic;
using TerraIntegration.DataStructures;
using TerraIntegration.UI;
using TerraIntegration.Values;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader;

namespace TerraIntegration.Variables
{
    public class EventSubscriber : Variable, IOwnProgrammerInterface
    {
        public override string Type => "eventsub";
        public override string TypeDisplay => "Event Subscriber";

        public override Type VariableReturnType => typeof(Values.Boolean);

        public override SpriteSheetPos SpriteSheetPos => new(BasicSheet, 0, 1);

        public Guid EventId { get; set; }
        public bool Triggered { get; set; }

        public UIPanel Interface { get; set; }
        public UIVariableSlot EventSlot { get; set; }
        public bool HasComplexInterface => false;

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

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            if (EventId != default)
                tooltips.Add(new(Mod, "TIEventId", $"[c/aaaa00:Event ID:] {World.Guids.GetShortGuid(EventId)}"));
        }

        public void SetupInterface()
        {
            Interface.Append(EventSlot = new()
            {
                Top = new(-21, .5f),
                Left = new(-21, .5f),
                DisplayOnly = true,

                VariableValidator = (var) => var is Variables.Event,
                HoverText = "Event"
            });
        }

        public Variable WriteVariable()
        {
            if (EventSlot?.Var?.Var is null) return null;
            return new EventSubscriber(EventSlot.Var.Var.Id);
        }
    }
}
