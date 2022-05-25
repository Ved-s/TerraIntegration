﻿using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    public class Boolean : VariableValue, IToString, IOwnProgrammerInterface
    {
        public override string Type => "bool";
        public override string TypeDisplay => "Boolean";

        public override Color TypeColor => Color.CadetBlue;

        public override SpriteSheetPos SpriteSheetPos => new(BasicSheet, 1, 0);

        public bool Value { get; set; }
        public UIPanel Interface { get; set; }
        public UITextPanel<string> InterfaceInput;
        public bool InterfaceValue;

        public Boolean() { }
        public Boolean(bool value) { Value = value; }

        private static HashSet<string> TrueValues = new() { "true", "1", "yes", "t" };
        private static HashSet<string> FalseValues = new() { "false", "0", "no", "f" };

        public override DisplayedValue Display() => new ColorTextDisplay(Value.ToString(), TypeColor);

        protected override VariableValue LoadCustomData(BinaryReader reader)
        {
            return new Boolean(reader.ReadBoolean());
        }

        protected override void SaveCustomData(BinaryWriter writer)
        {
            writer.Write(Value);
        }

        public override VariableValue GetFromCommand(CommandCaller caller, List<string> args)
        {
            if (args.Count < 1)
            {
                caller.Reply("Argument required: true/false value");
                return null;
            }

            bool v;
            string arg = args[0].ToLower();

            if (TrueValues.Contains(arg)) v = true;
            else if (FalseValues.Contains(arg)) v = false;
            else 
            {
                caller.Reply($"Value is not a boolean: {args[0]}");
                return null;
            }
            args.RemoveAt(0);

            return new Boolean(v);
        }

        public override string ToString()
        {
            return Value.ToString();
        }

        public override bool Equals(object obj)
        {
            return obj is Boolean boolean &&
                   Type == boolean.Type &&
                   Value == boolean.Value;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Type, Value);
        }

        public void SetupInterface()
        {
            Interface = new UIPanel();
            Interface.Append(InterfaceInput = new(InterfaceValue.ToString())
            {
                Width = new(80, 0),

                Top = new(-16, .5f),
                Left = new(-40, .5f),
                TextColor = TypeColor
            });
            InterfaceInput.OnClick += (ev, el) =>
            {
                SoundEngine.PlaySound(SoundID.MenuTick);
                InterfaceValue = !InterfaceValue;
                InterfaceInput.SetText(InterfaceValue.ToString());
            };
        }

        public void WriteVariable(Items.Variable var)
        {
            var.Var = new Constant(new Boolean(InterfaceValue));
        }
    }
}
