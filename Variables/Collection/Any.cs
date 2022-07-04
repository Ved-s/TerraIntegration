using System.Collections.Generic;
using System.Linq;
using TerraIntegration.Basic;
using TerraIntegration.DataStructures;
using TerraIntegration.Interfaces.Value;
using TerraIntegration.Templates;
using TerraIntegration.Values;

namespace TerraIntegration.Variables.Collection
{
    public class Any : DoubleReferenceVariable
    {
        public override ReturnType[] LeftSlotValueTypes => new ReturnType[] { typeof(ICollection) };
        public override string TypeName => "any";
        public override string TypeDefaultDisplayName => "Any";
        public override string TypeDefaultDescription => "Retutns true if collection contains any elements\nOr, if filter is present, when any value in collection matches it";

        public override ReturnType? VariableReturnType => typeof(Values.Boolean);

        public override SpriteSheetPos SpriteSheetPos => new(CollectionSheet, 2, 1);

        public override bool RightSlotOptional => true;

        public override ReturnType[] GetValidRightSlotTypes(ReturnType leftSlotType)
        {
            ReturnType collectionType = ICollection.TryGetCollectionType(LeftSlot?.Var?.Var) ?? typeof(VariableValue);
            return new ReturnType[] { Function.ReturnTypeOf(typeof(Values.Boolean), collectionType) };
        }

        public override Values.Boolean GetValue(ComponentSystem system, VariableValue left, VariableValue right, List<Error> errors)
        {
            ICollection collection = (ICollection)left;
            SpecialValue spec = right as SpecialValue;

            if (spec is null)
                return (collection.Enumerate(system, errors)?.Any()).ToBooleanValue();

            Function func = spec?.GetVariable<Function>(system, errors, TypeIdentity);
            if (func is null)
                return null;

            foreach (VariableValue value in collection.Enumerate(system, errors))
            {
                VariableValue condition = func.Execute(system, errors, value);
                if (condition is not Values.Boolean @bool)
                    return null;

                if (@bool.Value)
                    return new(true);
            }
            return new(false);
        }
    }
}
