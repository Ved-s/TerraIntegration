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
    public class Contains : DoubleReferenceVariableWithConst
    {
        public override Type[] LeftSlotValueTypes => new[] { typeof(ICollection) };
        public override string TypeName => "contains";
        public override string TypeDefaultDisplayName => "Contains";
        public override string TypeDefaultDescription => "Returns True if collection contains specified value";

        public override Type VariableReturnType => typeof(Values.Boolean);

        public override Type[] GetValidRightSlotTypes(Type leftSlotType)
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
