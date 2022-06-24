using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerraIntegration.Basic;
using TerraIntegration.Components;
using TerraIntegration.DataStructures;
using TerraIntegration.UI;
using TerraIntegration.Values;
using Terraria.GameContent.UI.Elements;

namespace TerraIntegration.Variables
{
    public class Event : Variable, IProgrammable
    {
        public override string TypeName => "event";
        public override string TypeDefaultDisplayName => "Event";
        public override string TypeDefaultDescription => "An event that will trigger all\nsubscribers for this event in system";

        public override ReturnType? VariableReturnType => SpecialValue.ReturnTypeOf<Event>();

        public override SpriteSheetPos SpriteSheetPos => new(BasicSheet, 3, 0);

        public UIPanel Interface { get; set; }
        public bool HasComplexInterface => false;

        private static HashSet<Point16> TriggeredPoints = new();

        public override VariableValue GetValue(ComponentSystem system, List<Error> errors)
        {
            return new SpecialValue(this);
        }



        private void Trigger(Point16 pos, ComponentSystem system)
        {
            if (TriggeredPoints.Contains(pos)) return;
            TriggeredPoints.Add(pos);

            foreach (var com in system.ComponentsWithVariables) 
            {
                if (com.Key == pos) continue;

                ComponentData data = com.Value?.GetDataOrNull(com.Key, false);
                if (data is null or SubTileComponentData) continue;

                foreach (var kvp in data.Variables)
                {
                    if (kvp.Value?.Var is EventSubscriber sub && sub.EventId == Id)
                    {
                        com.Value.OnEvent(com.Key, kvp.Key);
                        sub.Triggered = true;
                    }
                }
            }

            TriggeredPoints.Remove(pos);
        }

        public void SetupInterface()
        {
            Interface.Append(new UITextPanel("No variables needed") 
            {
                Width = new(0, 1),
                Height = new(0, 1),
                BackgroundColor = Color.Transparent,
                BorderColor = Color.Transparent,
            });
        }

        public Variable WriteVariable()
        {
            return new Event();
        }

        public static bool Trigger(Variable var, Point16 pos, ComponentSystem system, List<Error> errors)
        {
            if (var is null)
                return false;

            if (var is Event evt)
            {
                evt.Trigger(pos, system);
                return true;
            }

            if (var.VariableReturnType.MatchNull(SpecialValue.ReturnTypeOf<Event>()))
            {
                SpecialValue spec = var.GetValue(system, errors) as SpecialValue;
                if (spec is null)
                    return false;

                evt = spec.GetVariable(system, errors) as Event;
                if (evt is null)
                    return false;

                evt.Trigger(pos, system);
                return true;
            }

            return false;
        }
    }
}
