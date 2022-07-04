using System;
using System.Collections.Generic;
using System.Linq;
using TerraIntegration.Basic;
using TerraIntegration.DataStructures;
using TerraIntegration.Interfaces.Value;
using TerraIntegration.Templates;
using TerraIntegration.Values;

namespace TerraIntegration.Variables.Collection
{
    public class Skip : DoubleReferenceVariableWithConst
    {
        public override ReturnType[] LeftSlotValueTypes => new ReturnType[] { typeof(ICollection) };
        public override string TypeName => "skip";
        public override string TypeDefaultDisplayName => "Skip";
        public override string TypeDefaultDescription => "Skips specified amount of elements from the start of collection";
        
        public override SpriteSheetPos SpriteSheetPos => new(CollectionSheet, 3, 0);

        public ReturnType CollectionType => VariableReturnType?.SubType?.Length is null or 0 ? typeof(VariableValue) : VariableReturnType.Value.SubType[0];

        public override Type[] GetValidRightConstantSlotTypes(ReturnType leftSlotType) => new[] { typeof(Integer) };
        public override ReturnType[] GetValidRightReferenceSlotTypes(ReturnType leftSlotType) => new ReturnType[] { typeof(INumeric) };

        public override DoubleReferenceVariableWithConst CreateVariable(Variable left, ValueOrRef right)
        {
            return new Skip
            {
                VariableReturnType = new(typeof(ICollection), ICollection.TryGetCollectionType(left) ?? new ReturnType(typeof(VariableValue)))
            };
        }

        public override VariableValue GetValue(ComponentSystem system, VariableValue left, VariableValue right, List<Error> errors)
        {
            ICollection collection = (ICollection)left;
            int count = (int)((INumeric)right).NumericValue;

            return collection.Enumerate(system, errors).Skip(count).ToCollectionValue(CollectionType);
        }
    }
}
