using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerraIntegration.Components;
using TerraIntegration.Values;

namespace TerraIntegration.Variables
{
    public class Event : Variable
    {
        public override string Type => "event";
        public override string TypeDisplay => "Event";

        public override Type VariableReturnType => null;

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
                for (int i = 0; i < data.Variables.Length; i++)
                {
                    Items.Variable v = data.Variables[i];
                    if (v?.Var is EventSubscriber sub && sub.EventId == Id)
                    {
                        c.Component.OnEvent(c.Pos, i);
                        sub.Triggered = true;
                    }
                }
            }

            TriggeredPoints.Remove(pos);
        }
    }
}
