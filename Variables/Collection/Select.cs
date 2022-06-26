using Microsoft.Xna.Framework;
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
    public class Select : DoubleReferenceVariable
    {
        public override ReturnType[] LeftSlotValueTypes => new ReturnType[] { typeof(ICollection) };
        public override string TypeName => "select";
        public override string TypeDefaultDisplayName => "Select";
        public override string TypeDefaultDescription => "Modify each element of collection with function";

        public override SpriteSheetPos SpriteSheetPos => new(CollectionSheet, 3, 1);

        public ReturnType CollectionType => VariableReturnType?.SubType?.Length is null or 0 ? typeof(VariableValue) : VariableReturnType.Value.SubType[0];

        List<VariableValue> Result = new();
        
        public override ReturnType[] GetValidRightSlotTypes(ReturnType leftSlotType)
        {
            ReturnType collectionType = ICollection.TryGetCollectionType(LeftSlot?.Var?.Var) ?? typeof(VariableValue);
            return new ReturnType[] { Function.ReturnTypeOf(typeof(VariableValue), collectionType) };
        }

        public override DoubleReferenceVariable CreateVariable(Variable left, Variable right)
        {
            ReturnType collectionType = ICollection.TryGetCollectionType(LeftSlot?.Var?.Var) ?? typeof(VariableValue);

            if (!SpecialValue.ReturnTypeOf<Function>(typeof(VariableValue), collectionType).Match(right.VariableReturnType))
            {
                RightSlot?.NewFloatingText("Invalid selector", Color.Red);
                return null;
            }

            ReturnType returnType = right.VariableReturnType.Value.SubType[0].SubType[0];
            
            return new Select
            {
                VariableReturnType = new(typeof(ICollection), returnType)
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

                VariableValue newValue = func.Execute(system, errors, value);
                if (newValue is null)
                    return null;

                Result.Add(newValue);
            }

            return Result.ToCollectionValue(CollectionType);
        }
    }
}
