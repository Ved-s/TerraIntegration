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
    public class Contains : DoubleReferenceVariableWithConst
    {
        public override ReturnType[] LeftSlotValueTypes => new ReturnType[] { typeof(ICollection) };
        public override string TypeName => "contains";
        public override string TypeDefaultDisplayName => "Contains";
        public override string TypeDefaultDescription => "Returns True if collection contains specified value";

        public override SpriteSheetPos SpriteSheetPos => new(CollectionSheet, 1, 2);

        public override ReturnType? VariableReturnType => typeof(Values.Boolean);

        public override ReturnType[] GetValidRightSlotTypes(ReturnType leftSlotType)
        {
            if (LeftSlot?.Var is null) return null;
            return new[] { ICollection.TryGetCollectionType(LeftSlot.Var.Var) ?? typeof(VariableValue) };
        }

        public override VariableValue GetValue(ComponentSystem system, VariableValue left, VariableValue right, List<Error> errors)
        {
            ICollection collection = (ICollection)left;
            return new Values.Boolean(collection.Enumerate(system, errors).Any(v => v is not null && v.Equals(right)));
        }
    }
}
