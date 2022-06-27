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
using TerraIntegration.Interfaces.Math;
using TerraIntegration.Items;
using TerraIntegration.UI;
using TerraIntegration.Variables;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader;

namespace TerraIntegration.Values
{
    public class String : VariableValue, IToString, IAddable, Interfaces.ICollection<Char>, IEquatable, IProgrammable
    {
        public override string TypeName => "str";
        public override string TypeDefaultDisplayName => "String";
        public override string TypeDefaultDescription => "Text sequence of characters";

        public override Color TypeColor => Color.OrangeRed;

        public override SpriteSheetPos SpriteSheetPos => new(ValueSheet, 2, 1);

        public string Value { get; set; }
        public ReturnType[] ValidAddTypes => new ReturnType[] { typeof(IToString) };

        public UIPanel Interface { get; set; }
        public bool HasComplexInterface => false;

        public Type CollectionType => typeof(Char);

        UIFocusInputTextField InterfaceValue;

        public String() { }
        public String(string value) { Value = value; }

        public override DisplayedValue Display(ComponentSystem system) => new ColorTextDisplay(Value, TypeColor);

        protected override VariableValue LoadCustomData(BinaryReader reader)
        {
            return new String(reader.ReadString());
        }

        protected override void SaveCustomData(BinaryWriter writer)
        {
            writer.Write(Value);
        }

        public override VariableValue GetFromCommand(CommandCaller caller, List<string> args)
        {
            if (args.Count < 1)
            {
                caller.Reply("Argument required: string value");
                return null;
            }

            string arg = args[0];
            args.RemoveAt(0);

            return new String(arg);
        }

        public override string ToString()
        {
            return Value;
        }

        public VariableValue Add(VariableValue value, List<Error> errors, TypeIdentity id)
        {
            if (value is IToString toString)
                return new String(Value + toString.ToString());
            
            errors.Add(Errors.ExpectedValue(typeof(IToString), id));
            return null;
        }

        public override bool Equals(VariableValue obj)
        {
            return obj is String @string &&
                   TypeName == @string.TypeName &&
                   Value == @string.Value;
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
                Left = new(20, 0),
                Width = new(-40, 1),
                Height = new(25, 0),
            };
            Interface.Append(InterfaceValue);
        }

        public Basic.Variable WriteVariable()
        {
            return new Constant(new String(InterfaceValue.CurrentString.Replace("\\n", "\n")));
        }

        public IEnumerable<VariableValue> Enumerate(ComponentSystem system, List<Error> errors)
        {
            foreach (char c in Value)
                yield return new Char(c);
        }
    }
}
