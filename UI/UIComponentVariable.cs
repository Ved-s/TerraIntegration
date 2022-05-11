using TerraIntegration.Components;

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

                ComponentData data = Component.Component.GetData(Component.Pos);

                if (data.Variables.Length <= VariableSlot)
                    return null;

                return data.Variables[VariableSlot];
            }
            set
            {
                if (Component.Component is null)
                    return;

                ComponentData data = Component.Component.GetData(Component.Pos);

                if (data.Variables.Length <= VariableSlot)
                    return;

                data.Variables[VariableSlot] = value;
                Component.Component.OnVariableChanged(Component.Pos, VariableSlot);
            }
        }

        public PositionedComponent Component { get; set; }
        public int VariableSlot { get; set; }
    }
}
