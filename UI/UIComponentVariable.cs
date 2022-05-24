using TerraIntegration.Components;
using TerraIntegration.Variables;

namespace TerraIntegration.UI
{
    public class UIComponentVariable : UIVariable
    {
        public override Items.Variable Var
        {
            get
            {
                if (Component.Component is null)
                    return null;

                ComponentData data = Component.Component.GetDataOrNull(Component.Pos);

                return data?.GetVariableItem(VariableSlot);
            }
            set
            {
                if (Component.Component is null)
                    return;

                ComponentData data = Component.Component.GetData(Component.Pos);

                data.SetVariable(VariableSlot, value);
                Component.Component.OnVariableChanged(Component.Pos, VariableSlot);
            }
        }

        public PositionedComponent Component { get; set; }
        public string VariableSlot { get; set; }
    }
}
