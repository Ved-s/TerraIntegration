﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerraIntegration.Interfaces;
using TerraIntegration.Values;
using TerraIntegration.Variables;

namespace TerraIntegration.ValueProperties.Numeric
{
    public class ToShort : ValueConversion
    {
        public override Type ConvertFrom => typeof(INumeric);
        public override Type ConvertTo => typeof(Short);

        public override bool AppliesTo(VariableValue value) => value is not Short;
        
        public override VariableValue GetProperty(VariableValue value, List<Error> errors)
        {
            long v = ((INumeric)value).NumericValue;
            return (VariableValue.GetInstance<Short>() as INumeric).GetFromNumeric(v, errors);
        }
    }
}