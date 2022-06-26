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
using TerraIntegration.Variables;

namespace TerraIntegration.Variables.Collection
{
    public class First : DoubleReferenceVariable
    {
        public override ReturnType[] LeftSlotValueTypes => new ReturnType[] { typeof(ICollection) };
        public override string TypeName => "first";
        public override string TypeDefaultDisplayName => "First";
        public override string TypeDefaultDescription => "Retutns first matching element of the collection";

        public override SpriteSheetPos SpriteSheetPos => new(CollectionSheet, 0, 0);

        public ReturnType CollectionType => VariableReturnType?.SubType?.Length is null or 0 ? typeof(VariableValue) : VariableReturnType.Value.SubType[0];

        public override bool RightSlotOptional => true;

        public override DoubleReferenceVariable CreateVariable(Variable left, Variable right)
        {
            return new First
            {
                VariableReturnType = ICollection.TryGetCollectionType(left) ?? new ReturnType(typeof(VariableValue))
            };
        }

        public override ReturnType[] GetValidRightSlotTypes(ReturnType leftSlotType)
        {
            ReturnType collectionType = ICollection.TryGetCollectionType(LeftSlot?.Var?.Var) ?? typeof(VariableValue);
            return new ReturnType[] { Function.ReturnTypeOf(typeof(Values.Boolean), collectionType) };
        }

        public override VariableValue GetValue(ComponentSystem system, VariableValue left, VariableValue right, List<Error> errors)
        {
            ICollection collection = (ICollection)left;
            SpecialValue spec = right as SpecialValue;
            VariableValue result = null;

            if (spec is null)
            {
                result = collection.Enumerate(system, errors).FirstOrDefault();
            }
            else 
            {
                Function func = spec?.GetVariable<Function>(system, errors, TypeIdentity);
                if (func is null)
                    return null;

                foreach (VariableValue value in collection.Enumerate(system, errors))
                {
                    VariableValue condition = func.Execute(system, errors, value);
                    if (condition is not Values.Boolean @bool)
                        return null;

                    if (@bool.Value)
                    {
                        result = value;
                        break;
                    }
                }
            }

            if (result is null)
            {
                errors.Add(Errors.NoFilterMatches(Id));
                return null;
            }

            SetReturnTypeCache(result.GetReturnType());

            return result;
        }
    }
}
