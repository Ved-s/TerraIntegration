using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerraIntegration.Basic;
using TerraIntegration.DataStructures;
using TerraIntegration.Interfaces.Value;
using TerraIntegration.Templates;
using TerraIntegration.Values;
using TerraIntegration.Variables;

namespace TerraIntegration.ValueProperties.Numeric
{
    public class Decrement : ValueProperty
    {
        public override Type[] ValueTypes => new[] { typeof(INumeric) };
        public override string PropertyName => "dec";
        public override string PropertyDisplay => "Decrement";
        public override string PropertyDescription => "Decrements value by one";

        public override SpriteSheetPos SpriteSheetPos => new(MathSheet, 2, 0);

        public override ValueProperty CreateVariable(Variable var)
        {
            Decrement dec = new() { VariableId = var.Id };
            dec.SetReturnTypeCache(var.VariableReturnType);
            return dec;
        }

        public override VariableValue GetProperty(ComponentSystem system, VariableValue value, List<Error> errors)
        {
            INumeric numeric = (INumeric)value;

            long newVal = numeric.NumericValue - 1;
            return numeric.GetFromNumeric(newVal, errors);
        }
    }
}
