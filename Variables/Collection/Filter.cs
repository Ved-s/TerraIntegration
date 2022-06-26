using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerraIntegration.Basic;
using TerraIntegration.DataStructures;
using TerraIntegration.Interfaces;
using TerraIntegration.Templates;
using TerraIntegration.Values;

namespace TerraIntegration.Variables.Collection
{
    public class Filter : DoubleReferenceVariable
    {
        public override ReturnType[] LeftSlotValueTypes => new ReturnType[] { typeof(ICollection) };
        public override string TypeName => "where";
        public override string TypeDefaultDisplayName => "Filter";
        public override string TypeDefaultDescription => "Filter collection with function matcher";

        public override SpriteSheetPos SpriteSheetPos => new(CollectionSheet, 0, 1);

        public ReturnType CollectionType => VariableReturnType?.SubType?.Length is null or 0 ? typeof(VariableValue) : VariableReturnType.Value.SubType[0];

        List<VariableValue> Result = new();
        
        public override ReturnType[] GetValidRightSlotTypes(ReturnType leftSlotType)
        {
            ReturnType collectionType = ICollection.TryGetCollectionType(LeftSlot?.Var?.Var) ?? typeof(VariableValue);
            return new ReturnType[] { Function.ReturnTypeOf(typeof(Values.Boolean), collectionType) };
        }

        public override DoubleReferenceVariable CreateVariable(Variable left, Variable right)
        {
            return new Filter
            {
                VariableReturnType = new(typeof(ICollection), ICollection.TryGetCollectionType(left) ?? typeof(VariableValue))
            };
        }

        public override VariableValue GetValue(ComponentSystem system, VariableValue left, VariableValue right, List<Error> errors)
        {
            ICollection collection = (ICollection)left;
            SpecialValue spec = (SpecialValue)right;

            Function func = spec.GetVariable<Function>(system, errors, TypeIdentity);
            if (func is null)
                return null;

            Result.Clear();

            foreach (VariableValue value in collection.Enumerate(system, errors))
            {
                if (value is null)
                    return null;

                VariableValue condition = func.Execute(system, errors, value);
                if (condition is not Values.Boolean b)
                    return null;

                if (b.Value)
                    Result.Add(value);
            }

            return Result.ToCollectionValue(CollectionType);
        }
    }
}
