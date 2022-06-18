using System;
using System.Collections.Generic;
using System.Reflection;
using TerraIntegration.Basic;
using TerraIntegration.DataStructures;
using TerraIntegration.Interfaces;
using TerraIntegration.Templates;
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
        public override string PropertyDescription => "Returns value name";

        public override SpriteSheetPos SpriteSheetPos => new(BasicSheet, 0, 2);

        public override Type[] ValueTypes => new[] { typeof(INamed) };

        public override ReturnType? VariableReturnType => typeof(Values.String);

        public override VariableValue GetProperty(ComponentSystem system, VariableValue value, List<Error> errors)
        {
            return new Values.String((value as INamed).Name ?? "");
        }
    }
}
