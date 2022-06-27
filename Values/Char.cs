using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerraIntegration.Basic;
using TerraIntegration.DataStructures;
using TerraIntegration.DisplayedValues;
using TerraIntegration.Interfaces;
using TerraIntegration.Items;
using TerraIntegration.UI;
using TerraIntegration.Variables;
using Terraria.GameContent.UI.Elements;
using Terraria.Localization;
using Terraria.ModLoader;

namespace TerraIntegration.Values
{
    public class Char : VariableValue, IToString, IProgrammable
    {
        public override string TypeName => "char";
        public override string TypeDefaultDisplayName => "Char";
        public override string TypeDefaultDescription => "Single character";

        public override Color TypeColor => Color.LightPink;

        public override SpriteSheetPos SpriteSheetPos => new(ValueSheet, 3, 0);

        public char Value { get; set; }
        public UIPanel Interface { get; set; }
        public bool HasComplexInterface => false;

        UIFocusInputTextField InterfaceValue;

        public Char() { }
        public Char(char value) { Value = value; }

        public override DisplayedValue Display(ComponentSystem system) => new ColorTextDisplay(Value.ToString(), TypeColor);

        protected override VariableValue LoadCustomData(BinaryReader reader)
        {
            return new Char(reader.ReadChar());
        }

        protected override void SaveCustomData(BinaryWriter writer)
        {
            writer.Write(Value);
        }

        public override VariableValue GetFromCommand(CommandCaller caller, List<string> args)
        {
            if (args.Count < 1)
            {
                caller.Reply("Argument required: char value");
                return null;
            }

            if (!char.TryParse(args[0], out char val))
            {
                caller.Reply($"Value is not a char: {args[0]}");
                return null;
            }
            args.RemoveAt(0);

            return new Char(val);
        }

        public override string ToString()
        {
            return Value.ToString();
        }

        public override bool Equals(VariableValue obj)
        {
            return obj is Char @char &&
                   TypeName == @char.TypeName &&
                   Value == @char.Value;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(TypeName, Value);
        }

        public void SetupInterface()
        {
            InterfaceValue = new("")
            {
                Top = new(-12, .5f),
                Left = new(-20, .5f),
                Width = new(40, 0),
                Height = new(25, 0),

                TextHAlign = new(0, .5f),
                PaddingLeft = 0,

                ModifyTextInput = (@new, old) =>
                {
                    if (@new.Length == 0) return "";
                    return @new.Last().ToString();
                }
            };
            Interface.Append(InterfaceValue);
        }

        public Basic.Variable WriteVariable()
        {
            if (InterfaceValue.CurrentString.Length == 0)
            {
                InterfaceValue.NewFloatingText(TerraIntegration.Localize("ProgrammingErrors.CharStringEmpty"), Color.Red, 100, 1, new(.5f, 0));
                return null;
            }

            return new Constant(new Char(InterfaceValue.CurrentString[0]));
        }
    }
}
