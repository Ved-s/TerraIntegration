﻿using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TerraIntegration.Basic;
using TerraIntegration.DataStructures;
using TerraIntegration.Interfaces;
using TerraIntegration.Interfaces.Math;
using TerraIntegration.UI;
using TerraIntegration.Values;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader;

namespace TerraIntegration.Variables.Numeric
{
    public class Abs : ReferenceVariable
    {
        public override string Type => "abs";
        public override string TypeDisplay => "Absolute";

        public override Type[] ReferenceReturnTypes => new[] { typeof(INumeric), typeof(IDecimal) };

        public override VariableValue GetValue(VariableValue value, ComponentSystem system, List<Error> errors)
        {
            if (value is INumeric numeric)
                return numeric.GetFromNumeric(Math.Abs(numeric.NumericValue), errors);
            IDecimal dec = (IDecimal)value;
            return dec.GetFromDecimal(Math.Abs(dec.DecimalValue), errors);
        }

        public override ReferenceVariable CreateVariable(Variable var)
        {
            return new Abs()
            {
                VariableReturnType = var.VariableReturnType
            };
        }
    }
}