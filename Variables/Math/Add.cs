﻿using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TerraIntegration.Basic;
using TerraIntegration.DataStructures;
using TerraIntegration.Interfaces;
using TerraIntegration.Interfaces.Math;
using TerraIntegration.Templates;
using TerraIntegration.UI;
using TerraIntegration.Values;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader;

namespace TerraIntegration.Variables.Numeric
{
    public class Add : DoubleReferenceVariableWithConst
    {
        public override string TypeName => "add";
        public override string TypeDefaultDisplayName => "Add";
        public override string TypeDefaultDescription => "Adds two values together";

        public override SpriteSheetPos SpriteSheetPos => new(MathSheet, 0, 0);

        protected override VariableMatch InitLeftSlotMatch => VariableMatch.OfReturnType(typeof(IAddable));

        public override Type[] GetValidRightReferenceSlotTypes(Type leftSlotType)
        {
            if (VariableValue.ByType.TryGetValue(leftSlotType, out VariableValue value) && value is IAddable addable)
                return addable.ValidAddTypes;
            return null;
        }
        public override Type[] GetValidRightConstantSlotTypes(Type leftSlotType) => new[] { leftSlotType };

        public override VariableValue GetValue(ComponentSystem system, VariableValue left, VariableValue right, List<Error> errors)
        {
            IAddable addable = (IAddable)left;

            VariableValue result = addable.Add(right, errors, TypeIdentity);

            if (result is not null)
                SetReturnTypeCache(result.GetType());
            return result;
        }
        public override DoubleReferenceVariableWithConst CreateVariable(Variable left, ValueOrRef right)
        {
            return new Add()
            {
                VariableReturnType = left.VariableReturnType
            };
        }
    }
}
