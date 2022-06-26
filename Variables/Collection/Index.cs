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
    public class Index : DoubleReferenceVariableWithConst
    {
        public override ReturnType[] LeftSlotValueTypes => new ReturnType[] { typeof(ICollection) };
        public override string TypeName => "index";
        public override string TypeDefaultDisplayName => "Index";
        public override string TypeDefaultDescription => "Gets value from collection at specified index";

        public override SpriteSheetPos SpriteSheetPos => new(CollectionSheet, 1, 0);

        List<VariableValue> ValueList;

        public override Type[] GetValidRightConstantSlotTypes(ReturnType leftSlotType) => new[] { typeof(Integer) };
        public override ReturnType[] GetValidRightReferenceSlotTypes(ReturnType leftSlotType) => new ReturnType[] { typeof(INumeric) };

        public override DoubleReferenceVariableWithConst CreateVariable(Variable left, ValueOrRef right)
        {
            return new Index
            {
                VariableReturnType = ICollection.TryGetCollectionType(left) ?? typeof(VariableValue)
            };
        }

        public override VariableValue GetValue(ComponentSystem system, VariableValue left, VariableValue right, List<Error> errors)
        {
            if (ValueList is null) ValueList = new();

            ICollection collection = (ICollection)left;
            int index = (int)((INumeric)right).NumericValue;
            try
            {
                ValueList.AddRange(collection.Enumerate(system, errors));

                if (index < 0)
                    index += ValueList.Count;

                if (index < 0 || index >= ValueList.Count)
                {
                    errors.Add(Errors.IndexOutOfBounds(index, TypeIdentity, ValueList.Count));
                    return null;
                }
                return ValueList[index];
            }
            finally 
            {
                ValueList.Clear();
            }
        }
    }
}
