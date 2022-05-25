﻿using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerraIntegration.Values;
using TerraIntegration.Variables;

namespace TerraIntegration.ValueProperties.String
{
    public class Length : ValueProperty<Values.String>
    {
        public override string PropertyName => "length";
        public override string PropertyDisplay => "Length";
        public override Type VariableReturnType => typeof(Integer);

        public override SpriteSheetPos SpriteSheetPos => new(StringSheet, 1, 0);

        public override VariableValue GetProperty(Values.String value, List<Error> errors)
        {
            return new Integer(value.Value?.Length ?? 0);
        }
    }
}