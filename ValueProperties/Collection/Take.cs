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
        public override Type[] LeftSlotValueTypes => new[] { typeof(ICollection) };
        public override string TypeName => "take";
        public override string TypeDefaultDisplayName => "Take";
        public override string TypeDefaultDescription => "Gets specified amount of values from start collection";

        public Type CollectionType => collectionType ??= ICollection.TryGetCollectionType(VariableReturnType);

        private Type collectionType;

        public override Type[] GetValidRightConstantSlotTypes(Type leftSlotType) => new[] { typeof(Integer) };
        public override Type[] GetValidRightReferenceSlotTypes(Type leftSlotType) => new[] { typeof(INumeric) };

        public override DoubleReferenceVariableWithConst CreateVariable(Variable left, ValueOrRef right)
        {
            return new Take
            {
                VariableReturnType = ICollection.OfType(ICollection.TryGetCollectionType(left) ?? typeof(VariableValue)),
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
