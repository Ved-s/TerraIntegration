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
    public class Modulo : DoubleReferenceVariable
    {
        public override string Type => "mod";
        public override string TypeDisplay => "Modulo";

        public override SpriteSheetPos SpriteSheetPos => new(MathSheet, 2, 1);

        public override Type[] LeftSlotValueTypes => new[] { typeof(IModulable) };

        public Modulo() { }
        public Modulo(Guid left, Guid right)
        {
            LeftId = left;
            RightId = right;
        }
        public override Type[] GetValidRightSlotTypes(Type leftSlotType)
        {
            if (VariableValue.ByType.TryGetValue(leftSlotType, out VariableValue value) && value is IModulable modulable)
                return modulable.ValidModuloTypes;
            return null;
        }
        public override VariableValue GetValue(ComponentSystem system, VariableValue left, VariableValue right, List<Error> errors)
        {
            IModulable modulable = (IModulable)left;

            VariableValue result = modulable.Modulo(right, errors);

            if (result is not null)
                SetReturnTypeCache(result.GetType());
            return result;
        }
        public override DoubleReferenceVariable CreateVariable(Variable left, Variable right)
        {
            return new Modulo()
            {
                VariableReturnType = left.VariableReturnType
            };
        }
    }
}
