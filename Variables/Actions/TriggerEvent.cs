using System.Collections.Generic;
using TerraIntegration.Basic;
using TerraIntegration.DataStructures;

namespace TerraIntegration.Variables.Actions
{
    public class TriggerEvent : ActionVariable
    {
        public override VariableMatch Variables => VariableMatch.OfType<Event>(true);
        public override bool NeedsSaveTag => true;
        public override bool HasComplexInterface => false;

        public override string TypeName => "trigEvt";
        public override string TypeDefaultDisplayName => "Trigger event";
        public override string TypeDefaultDescription => "Triggers an event on execution";

        public override SpriteSheetPos SpriteSheetPos => new(ActionsSheet, 2, 0);

        public override void Execute(Point16 pos, Variable var, ComponentSystem system, List<Error> errors)
        {
            Event.Trigger(var, pos, system, errors);
        }
    }
}
