namespace TerraIntegration.UI
{
    public class UIComponentVariable : UIVariable
    {
        public override Items.Variable Var
        {
            get
            {
                if (Component.Component.VariableSlots <= VariableSlot)
                    return null;

                return Component.Component.GetData(Component.Pos).Variables[VariableSlot];
            }
            set
            {
                if (Component.Component.VariableSlots <= VariableSlot)
                    return;

                Component.Component.GetData(Component.Pos).Variables[VariableSlot] = value;
                Component.Component.OnVariableChanged(Component.Pos, VariableSlot);
            }
        }

        public PositionedComponent Component { get; set; }
        public int VariableSlot { get; set; }
    }
}
