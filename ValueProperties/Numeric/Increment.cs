﻿using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerraIntegration.Interfaces;
using TerraIntegration.Values;
using TerraIntegration.Variables;

namespace TerraIntegration.ValueProperties.Numeric
{
    public class Increment : ValueProperty
    {
        public override Type ValueType => typeof(INumeric);
        public override string PropertyName => "inc";
        public override string PropertyDisplay => "Increment";

        public override SpriteSheet SpriteSheet => MathSheet;
        public override Point SpritesheetPos => new(1, 0);

        public override ValueProperty CreateVariable(Variable var)
        {
            Increment inc = new() { VariableId = var.Id };
            inc.SetReturnTypeCache(var.VariableReturnType);
            return inc;
        }

        public override VariableValue GetProperty(VariableValue value, List<Error> errors)
        {
            INumeric numeric = (INumeric)value;

            long newVal = numeric.NumericValue + 1;
            return numeric.FromNumeric(newVal, errors);
        }
    }
}