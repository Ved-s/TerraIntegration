using System;
using System.Collections.Generic;
using System.Linq;
using TerraIntegration.Basic;
using TerraIntegration.DataStructures;
using TerraIntegration.Interfaces;
using TerraIntegration.Templates;
using TerraIntegration.Values;

namespace TerraIntegration.ValueProperties.Collection
{
    public class Take : DoubleReferenceVariableWithConst
    {
        public override ReturnType[] LeftSlotValueTypes => new ReturnType[] { typeof(ICollection) };
        public override string TypeName => "take";
        public override string TypeDefaultDisplayName => "Take";
        public override string TypeDefaultDescription => "Gets specified amount of values from start collection";
        
        public override SpriteSheetPos SpriteSheetPos => new(CollectionSheet, 3, 0);

        public ReturnType CollectionType => collectionType ??= ICollection.TryGetCollectionType(VariableReturnType?.Type);

        private ReturnType? collectionType;

        public override Type[] GetValidRightConstantSlotTypes(ReturnType leftSlotType) => new[] { typeof(Integer) };
        public override ReturnType[] GetValidRightReferenceSlotTypes(ReturnType leftSlotType) => new ReturnType[] { typeof(INumeric) };

        public override DoubleReferenceVariableWithConst CreateVariable(Variable left, ValueOrRef right)
        {
            return new Take
            {
                VariableReturnType = ICollection.OfType(ICollection.TryGetCollectionType(left)?.Type ?? typeof(VariableValue)),
            };
        }

        public override VariableValue GetValue(ComponentSystem system, VariableValue left, VariableValue right, List<Error> errors)
        {
            ICollection collection = (ICollection)left;
            int count = (int)((INumeric)right).NumericValue;

            return collection.Enumerate(system, errors).Take(count).ToCollectionValue(CollectionType);
        }
    }
}
