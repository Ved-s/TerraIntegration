﻿using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TerraIntegration.Basic;
using TerraIntegration.Interfaces;
using TerraIntegration.Interfaces.Math;
using TerraIntegration.UI;
using TerraIntegration.Values;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader;

namespace TerraIntegration.Variables.Numeric
{
    public class Multiply : DoubleReferenceVariable
    {
        public override string Type => "mul";
        public override string TypeDisplay => "Multiply";

        public override SpriteSheetPos SpriteSheetPos => new(MathSheet, 0, 1);

        public override Type[] LeftSlotValueTypes => new[] { typeof(IMultipliable) };

        public Multiply() { }
        public Multiply(Guid left, Guid right)
        {
            LeftId = left;
            RightId = right;
        }
        public override Type[] GetValidRightSlotTypes(Type leftSlotType)
        {
            if (VariableValue.ByType.TryGetValue(leftSlotType, out VariableValue value) && value is IMultipliable multipliable)
                return multipliable.ValidMultiplyTypes;
            return null;
        }
        public override VariableValue GetValue(ComponentSystem system, VariableValue left, VariableValue right, List<Error> errors)
        {
            IMultipliable multipliable = (IMultipliable)left;

            VariableValue result = multipliable.Multiply(right, errors);

            if (result is not null)
                SetReturnTypeCache(result.GetType());
            return result;
        }
        public override DoubleReferenceVariable CreateVariable(Variable left, Variable right)
        {
            return new Multiply()
            {
                VariableReturnType = left.VariableReturnType
            };
        }
    }
}
