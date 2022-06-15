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
    public class CollectionList : VariableValue, Interfaces.ICollection
    {
        public override string TypeName => "colList";
        public override string TypeDefaultDisplayName => $"{TypeToName<Interfaces.ICollection>()} of {TypeToName(CollectionType, true)}";
        public Type CollectionType { get; private set; }

        public IEnumerable<VariableValue> Enumerable { get; private set; }

        List<string> Strings;

        public CollectionList() { }
        public CollectionList(IEnumerable<VariableValue> enumerable, Type collectionType)
        {
            Enumerable = enumerable;
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
            writer.Write(TypeToString(CollectionType) ?? "");

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

                return new ColorTextDisplay($"[{(anyLong? "\n" : "")} {string.Join(delim, Strings)}{(anyLong ? "\n" : " ")}]", Color.White)
                {
                    TextAlign = new(0, .5f)
                };
            }
            finally 
            {
                Strings.Clear();
            }
        }
    }
}
