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
using Terraria.Audio;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.ModLoader;

namespace TerraIntegration.Values
{
    public class Boolean : VariableValue, IToString, IEquatable, IOwnProgrammerInterface
    {
        public override string TypeName => "bool";
        public override string TypeDefaultDisplayName => "Boolean";
        public override string TypeDefaultDescription => "Two-state value, either True or False";

        public override Color TypeColor => Color.CadetBlue;

        public override SpriteSheetPos SpriteSheetPos => new(BasicSheet, 1, 0);

        public bool Value { get; set; }
        public UIPanel Interface { get; set; }
        public bool HasComplexInterface => false;

        public UITextPanel<string> InterfaceInput;
        public bool InterfaceValue;

        public Boolean() { }
        public Boolean(bool value) { Value = value; }

        private static HashSet<string> TrueValues = new() { "true", "1", "yes", "t" };
        private static HashSet<string> FalseValues = new() { "false", "0", "no", "f" };

        public override DisplayedValue Display(ComponentSystem system) => new ColorTextDisplay(Value.ToString(), TypeColor);

        protected override VariableValue LoadCustomData(BinaryReader reader)
        {
            return new Boolean(reader.ReadBoolean());
        }
        protected override void SaveCustomData(BinaryWriter writer)
        {
            writer.Write(Value);
        }

        public override string ToString()
        {
            return Value.ToString();
        }

        public override bool Equals(VariableValue obj)
        {
            return obj is Boolean boolean &&
                   TypeName == boolean.TypeName &&
                   Value == boolean.Value;
        }
        public override int GetHashCode()
        {
            return HashCode.Combine(TypeName, Value);
        }

        public void SetupInterface()
        {
            Interface.Append(InterfaceInput = new(InterfaceValue.ToString())
            {
                Width = new(80, 0),

                Top = new(-15, .5f),
                Left = new(-40, .5f),
                TextColor = TypeColor,
                PaddingTop = 8,
                PaddingBottom = 0,
                Height = new(30, 0)
            });
            InterfaceInput.OnClick += (ev, el) =>
            {
                SoundEngine.PlaySound(SoundID.MenuTick);
                InterfaceValue = !InterfaceValue;
                InterfaceInput.SetText(InterfaceValue.ToString());
            };
        }
        public Basic.Variable WriteVariable()
        {
            return new Constant(new Boolean(InterfaceValue));
        }
    }
}
