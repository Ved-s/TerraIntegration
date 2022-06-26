using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;
using TerraIntegration.Basic;
using TerraIntegration.DataStructures;
using TerraIntegration.UI;
using Terraria.GameContent.UI.Elements;

namespace TerraIntegration.Variables
{
    public class FunctionArgument : Variable, IProgrammable
    {
        public override string TypeName => "funcArg";
        public override string TypeDefaultDisplayName => "Function argument";
        public override string TypeDefaultDescription => "Returns function argument value";

        public override SpriteSheetPos SpriteSheetPos => new(BasicSheet, 2, 3);

        UIVariableSwitch TypeSwitch;
        public UIPanel Interface { get; set; }
        public bool HasComplexInterface => false;

        public override VariableValue GetValue(ComponentSystem system, List<Error> errors)
        {
            if (!system.FunctionArguments.TryGetValue(Id, out VariableValue value))
                value = null;

            if (value is null)
                errors.Add(Errors.FunctionArgumentNotSet(Id));

            return value;
        }

        public void SetupInterface()
        {
            Interface.Append(TypeSwitch = new()
            {
                Top = new(-16, .5f),
                Left = new(-16, .5f),

                SwitchValues = VariableValue.ByType.Values
                    .Where(v => !v.HideInProgrammer)
                    .Select(v => new ValueVariablePair(v.GetReturnType(), null))
                    .Append(new(null, null, null, true))
                    .ToArray()
            });

            Interface.Append(new UITextPanel()
            {
                Top = new(-48, .5f),
                Left = new(0, 0),
                Width = new(0, 1),
                Height = new(32, 0),

                PaddingTop = 0,
                PaddingBottom = 0,

                Text = "Argument type:",

                BackgroundColor = Color.Transparent,
                BorderColor = Color.Transparent,

            });
        }

        public Variable WriteVariable()
        {
            ReturnType? type = TypeSwitch?.Current?.ValueType;

            if (type is null)
                return null;

            return new FunctionArgument
            {
                VariableReturnType = type.Value
            };
        }
    }
}
