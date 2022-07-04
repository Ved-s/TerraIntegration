using System;
using System.Collections.Generic;
using TerraIntegration.Basic;
using TerraIntegration.DataStructures;
using TerraIntegration.Interfaces.Value;
using TerraIntegration.Templates;
using TerraIntegration.Values;

namespace TerraIntegration.Variables.Collection
{
    public class IndexOf : DoubleReferenceVariableWithConst
    {
        public override ReturnType[] LeftSlotValueTypes => new ReturnType[] { typeof(ICollection) };
        public override string TypeName => "indexof";
        public override string TypeDefaultDisplayName => "Index Of";
        public override string TypeDefaultDescription => "Retutns first matching element index in the collection or -1 if none";

        public override ReturnType? VariableReturnType => typeof(Values.Integer);

        public override SpriteSheetPos SpriteSheetPos => new(CollectionSheet, 0, 2);

        public override Type[] GetValidRightConstantSlotTypes(ReturnType leftSlotType)
        {
            Type collectionType = ICollection.TryGetCollectionType(LeftSlot?.Var?.Var)?.Type;
            return collectionType is null ? null : new Type[] { collectionType };
        }

        public override ReturnType[] GetValidRightSlotTypes(ReturnType leftSlotType)
        {
            ReturnType type = ICollection.TryGetCollectionType(LeftSlot?.Var?.Var) ?? typeof(VariableValue);
            return new[] { type, Function.ReturnTypeOf(typeof(Values.Boolean), type) };
        }

        public override VariableValue GetValue(ComponentSystem system, VariableValue left, VariableValue right, List<Error> errors)
        {
            ICollection collection = (ICollection)left;

            int index = 0;

            if (right is SpecialValue spec)
            {
                Function matcher = spec.GetVariable<Function>(system, errors, TypeIdentity);
                if (matcher is null)
                    return null;

                foreach (VariableValue val in collection.Enumerate(system, errors))
                {
                    VariableValue condition = matcher.Execute(system, errors, val);
                    if (condition is not Values.Boolean @bool)
                        return null;

                    if (@bool.Value)
                        return new Integer(index);

                    index++;
                }
                return new Integer(-1);
            }

            foreach (VariableValue value in collection.Enumerate(system, errors))
            {
                if (right.Equals(value))
                    return new Integer(index);
                
                index++;
            }
            return new Integer(-1);
        }
    }
}
