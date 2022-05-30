using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerraIntegration.Basic;
using TerraIntegration.Components;
using TerraIntegration.DataStructures;
using TerraIntegration.Values;

namespace TerraIntegration.Variables
{
    public class Event : Variable
    {
        public override string Type => "event";
        public override string TypeDisplay => "Event";

        public override Type VariableReturnType => null;

        public override SpriteSheetPos SpriteSheetPos => new(BasicSheet, 3, 0);

        private static HashSet<Point16> TriggeredPoints = new();

        public override VariableValue GetValue(ComponentSystem system, List<Error> errors)
        {
            return null;
        }

        public void Trigger(Point16 pos, ComponentSystem system)
        {
            if (TriggeredPoints.Contains(pos)) return;
            TriggeredPoints.Add(pos);

            foreach (PositionedComponent c in system.ComponentsWithVariables) 
            {
                if (c.Pos == pos) continue;

                ComponentData data = c.GetData();
                foreach (var kvp in data.Variables)
                {
                    if (kvp.Value?.Var is EventSubscriber sub && sub.EventId == Id)
                    {
                        c.Component.OnEvent(c.Pos, kvp.Key);
                        sub.Triggered = true;
                    }
                }
            }

            TriggeredPoints.Remove(pos);
        }
    }
}
