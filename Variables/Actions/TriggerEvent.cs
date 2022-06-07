using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerraIntegration.Basic;
using TerraIntegration.DataStructures;

namespace TerraIntegration.Variables.Actions
{
    public class TriggerEvent : ActionVariable
    {
        public override VariableMatch Variables => VariableMatch.OfType<Event>();
        public override bool NeedsSaveTag => true;
        public override bool HasComplexInterface => false;

        public override string TypeName => "trigEvt";
        public override string TypeDefaultDisplayName => "Trigger event";

        public override SpriteSheetPos SpriteSheetPos => new(ActionsSheet, 2, 0);

        public override void Execute(Point16 pos, Variable var, ComponentSystem system, List<Error> errors)
        {
            if (var is Event evt)
                evt.Trigger(pos, system);
        }
    }
}
