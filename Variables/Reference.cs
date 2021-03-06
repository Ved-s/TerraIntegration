using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerraIntegration.Basic;
using TerraIntegration.DataStructures;
using TerraIntegration.Templates;
using TerraIntegration.Values;
using Terraria.ModLoader;

namespace TerraIntegration.Variables
{
    public class Reference : ReferenceVariable
    {
        public override string TypeName => "ref";
        public override string TypeDefaultDisplayName => "Reference";
        public override string TypeDefaultDescription => "Returns referenced variable's value";

        public override SpriteSheetPos SpriteSheetPos => new(BasicSheet, 1, 0);

        public Reference() { }
        public Reference(Guid varId)
        {
            VariableId = varId;
        }

        public override VariableValue GetValue(VariableValue value, ComponentSystem system, List<Error> errors)
        {
            SetReturnTypeCache(value.GetReturnType());
            return value;
        }

        public override ReferenceVariable CreateVariable(Variable var)
        {
            return new Reference()
            {
                VariableReturnType = var.VariableReturnType
            };
        }
    }
}
