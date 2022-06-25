using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerraIntegration.Basic;
using TerraIntegration.DataStructures;

namespace TerraIntegration.Values
{
    public class SpecialValue : VariableValue
    {
        public override string TypeName => "special";
        public override string TypeDefaultDisplayName => "Variable";

        public override SpriteSheetPos SpriteSheetPos => new(BasicSheet, 0, 3);

        public Type VariableType { get; set; }
        public ReturnType[] VariableSubTypes { get; set; }
        public Guid VariableId { get; set; }

        public override bool HideInProgrammer => true;

        public SpecialValue() { }
        public SpecialValue(Variable var, params ReturnType[] subTypes) 
        {
            VariableType = var.GetType();
            VariableSubTypes = subTypes;
            VariableId = var.Id;
        }

        public override string FormatReturnSubtypes(ReturnType[] subTypes, bool colored)
        {
            string sub = subTypes[0].SubType is null ? null : string.Join(", ", subTypes[0].SubType.Select(t => t.ToStringName(colored)));

            string name = Variable.ByType.TryGetValue(subTypes[0].Type, out Variable var) ? var.TypeDisplayName : "Unregistered";

            return " " + name + (sub.IsNullEmptyOrWhitespace() ? "" : " of " + sub);
        }

        public override bool Equals(VariableValue value)
        {
            return value is SpecialValue spec &&
                VariableType == spec.VariableType &&
                VariableId == spec.VariableId &&
                Util.ObjectsNullEqual(VariableSubTypes, spec.VariableSubTypes);
        }

        public Variable GetVariable(ComponentSystem system, List<Error> errors)
        {
            return system?.GetVariable(VariableId, errors);
        }

        public T GetVariable<T>(ComponentSystem system, List<Error> errors, TypeIdentity id) where T : Variable
        {
            return system?.GetVariable<T>(VariableId, errors, id);
        }

        public static T GetVariableOrNull<T>(Variable var, ComponentSystem system, List<Error> errors) where T : Variable
        {
            if (var is null)
                return null;

            if (var is T tvar)
                return tvar;

            if (!var.VariableReturnType.MatchNull(ReturnTypeOf<T>()))
                return null;

            SpecialValue spec = var.GetValue(system, errors) as SpecialValue;
            if (spec is null)
                return null;

            return spec.GetVariable(system, errors) as T;
        }

        public override ReturnType GetReturnType()
        {
            return ReturnTypeOf(VariableType, VariableSubTypes);
        }

        public static ReturnType ReturnTypeOf<TVar>(params ReturnType[] subTypes) where TVar : Variable
        {
            return new(typeof(SpecialValue), new ReturnType(typeof(TVar), subTypes));
        }
        public static ReturnType ReturnTypeOf(Type variable, params ReturnType[] subTypes)
        {
            return new(typeof(SpecialValue), new ReturnType(variable, subTypes));
        }
    }
}
