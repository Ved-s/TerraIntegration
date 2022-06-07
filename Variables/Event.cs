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
    public class Event : Variable, IOwnProgrammerInterface
    {
        public override string TypeName => "event";
        public override string TypeDefaultDisplayName => "Event";

        public override Type VariableReturnType => null;

        public override SpriteSheetPos SpriteSheetPos => new(BasicSheet, 3, 0);

        public UIPanel Interface { get; set; }
        public bool HasComplexInterface => false;

        private static HashSet<Point16> TriggeredPoints = new();

        public override VariableValue GetValue(ComponentSystem system, List<Error> errors)
        {
            return null;
        }

        public void Trigger(Point16 pos, ComponentSystem system)
        {
            if (TriggeredPoints.Contains(pos)) return;
            TriggeredPoints.Add(pos);

            foreach (var com in system.ComponentsWithVariables) 
            {
                if (com.Key == pos) continue;

                ComponentData data = com.Value?.GetDataOrNull(com.Key);
                if (data is null) continue;

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
    }
}
