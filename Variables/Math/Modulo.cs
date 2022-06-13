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
    public class Modulo : DoubleReferenceVariableWithConst
    {
        public override string TypeName => "mod";
        public override string TypeDefaultDisplayName => "Modulo";
        public override string TypeDefaultDescription => "Modulus left value by right value";

        public override SpriteSheetPos SpriteSheetPos => new(MathSheet, 2, 1);

        protected override VariableMatch InitLeftSlotMatch => VariableMatch.OfReturnType(typeof(IModulable));

        public override Type[] GetValidRightReferenceSlotTypes(Type leftSlotType)
        {
            if (VariableValue.ByType.TryGetValue(leftSlotType, out VariableValue value) && value is IModulable modulable)
                return modulable.ValidModuloTypes;
            return null;
        }
        public override Type[] GetValidRightConstantSlotTypes(Type leftSlotType) => new[] { leftSlotType };

        public override VariableValue GetValue(ComponentSystem system, VariableValue left, VariableValue right, List<Error> errors)
        {
            IModulable modulable = (IModulable)left;

            VariableValue result = modulable.Modulo(right, errors, TypeIdentity);

            if (result is not null)
                SetReturnTypeCache(result.GetType());
            return result;
        }
        public override DoubleReferenceVariableWithConst CreateVariable(Variable left, ValueOrRef right)
        {
            return new Modulo()
            {
                VariableReturnType = left.VariableReturnType
            };
        }
    }
}
