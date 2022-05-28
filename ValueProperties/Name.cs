using System;
using System.Collections.Generic;
using System.Reflection;
using TerraIntegration.Interfaces;
using TerraIntegration.Values;
using TerraIntegration.Variables;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace TerraIntegration.ComponentProperties.TileReader
{
    public class Name : ValueProperty
    {
        public override string PropertyName => "name";
        public override string PropertyDisplay => "Name";

        public override Type ValueType => typeof(INamed);

        public override Type VariableReturnType => typeof(Values.String);

        public override VariableValue GetProperty(VariableValue value, List<Error> errors)
        {
            return new Values.String((value as INamed).Name ?? "");
        }
    }
}
