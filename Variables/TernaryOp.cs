using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerraIntegration.Basic;
using TerraIntegration.DataStructures;
using TerraIntegration.UI;
using Terraria.GameContent.UI.Elements;

namespace TerraIntegration.Variables
{
    public class TernaryOp : Variable, IOwnProgrammerInterface
    {
        public override string Type => "tern";
        public override string TypeDisplay => "Ternary operator";

        public override Type[] RelatedTypes => new[] { typeof(Values.Boolean) };

        public Guid Condition, TrueValue, FalseValue;

        public UIPanel Interface { get; set; }
        public bool HasComplexInterface => false;

        public UIVariableSlot ConditionSlot, TrueSlot, FalseSlot;

        public TernaryOp() { }
        public TernaryOp(Guid condition, Guid @true, Guid @false)
        {
            Condition = condition;
            TrueValue = @true;
            FalseValue = @false;
        }

        public override VariableValue GetValue(ComponentSystem system, List<Error> errors)
        {
            if (!TryGetValueType<Values.Boolean>(system.GetVariableValue(Condition, errors), errors, out var condition)) 
                return null;

            return condition.Value ? system.GetVariableValue(TrueValue, errors) : system.GetVariableValue(FalseValue, errors);
        }

        public void SetupInterface()
        {
            Interface.Append(TrueSlot = new()
            {
                DisplayOnly = true,
                Top = new(-21, .5f),
                Left = new(-71, .5f),

                VariableValidator = (var) => var.VariableReturnType == typeof(Values.Boolean),
                HoverText = $"A {VariableValue.TypeToName<Values.Boolean>()} condition value"
            });

            Interface.Append(TrueSlot = new()
            {
                DisplayOnly = true,
                Top = new(-21, .5f),
                Left = new(-21, .5f),

                VariableValidator = (var) => FalseSlot.Var is null || var.VariableReturnType == FalseSlot.Var.Var.VariableReturnType,
                HoverText = "Value, which is returned if condition is True\nShould be the same type as False value"
            });

            Interface.Append(FalseSlot = new()
            {
                DisplayOnly = true,
                Top = new(-21, .5f),
                Left = new(50, .5f),

                VariableValidator = (var) => TrueSlot.Var is null || var.VariableReturnType == TrueSlot.Var.Var.VariableReturnType,
                HoverText = "Value, which is returned if condition is False\nShould be the same type as True value"
            });
        }

        public Variable WriteVariable()
        {
            if (ConditionSlot.Var is null || TrueSlot.Var is null || FalseSlot.Var is null) 
                return null;

            return new TernaryOp(ConditionSlot.Var.Var.Id, TrueSlot.Var.Var.Id, FalseSlot.Var.Var.Id);
        }
    }
}
