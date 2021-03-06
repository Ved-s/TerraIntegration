using Microsoft.VisualBasic;
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

namespace TerraIntegration.Values
{
    public class CollectionList : VariableValue, Interfaces.Value.ICollection
    {
        public override string TypeName => "colList";
        public override string TypeDefaultDisplayName => $"{TypeToName<Interfaces.Value.ICollection>()} of {CollectionType.ToStringName(true)}";
        public ReturnType CollectionType { get; private set; }

        public override SpriteSheetPos SpriteSheetPos => new(ValueSheet, 0, 2);

        public IEnumerable<VariableValue> Enumerable { get; private set; }

        public override bool HideInProgrammer => true;

        List<string> Strings;

        public CollectionList() { }
        public CollectionList(IEnumerable<VariableValue> enumerable, ReturnType collectionType)
        {
            Enumerable = enumerable.ToArray();
            CollectionType = collectionType;
        }

        public override bool Equals(VariableValue value)
        {
            return value is CollectionList cl && cl.Enumerable.SequenceEqual(Enumerable);
        }

        public IEnumerable<VariableValue> Enumerate(ComponentSystem system, List<Error> errors)
        {
            return Enumerable;
        }

        public override VariableValue Clone()
        {
            return new CollectionList(Enumerable.ToArray(), CollectionType);
        }

        protected override void SaveCustomData(BinaryWriter writer)
        {
            writer.Write(CollectionType.ToTypeString() ?? "");

            List<VariableValue> ienum = new(Enumerable);

            writer.Write((ushort)ienum.Count());
            foreach (VariableValue value in ienum)
                SaveData(value, writer);
            
        }
        protected override VariableValue LoadCustomData(BinaryReader reader)
        {
            Type collectionType = typeof(VariableValue);
            string type = reader.ReadString();
            if (!type.IsNullEmptyOrWhitespace())
                collectionType = StringToType(type);

            VariableValue[] values = new VariableValue[reader.ReadInt16()];
            for (int i = 0; i < values.Length; i++)
                LoadData(reader);

            return new CollectionList(values, collectionType);
        }

        public override DisplayedValue Display(ComponentSystem system)
        {
            if (Strings is null)
                Strings = new();

            try
            {
                bool anyLong = false;

                foreach (var v in Enumerable)
                {
                    string hover = v?.Display(system).HoverText;
                    Strings.Add(hover);
                    if (hover is not null && hover.Length > 15)
                        anyLong = true;
                }

                string delim = anyLong ? ",\n " : ", ";

                string longText = $"[{(anyLong ? "\n" : "")} {string.Join(delim, Strings)}{(anyLong ? "\n" : " ")}]";
                int shortCount = anyLong ? 2 : 5;
                string shortText = $"[ {string.Join(", ", Strings.Take(shortCount))}{(Strings.Count > shortCount? "..." : "")} ]";

                return new ColorTextDisplay(longText, Color.White, shortText)
                {
                    TextAlign = new(0, .5f)
                };
            }
            finally 
            {
                Strings.Clear();
            }
        }

        public override ReturnType GetReturnType()
        {
            return new ReturnType(typeof(Interfaces.Value.ICollection), CollectionType);
        }
    }
}
