using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TerraIntegration.Interfaces;

namespace TerraIntegration.Values
{
    public class List : VariableValue, ICollection
    {
        public override string Type => "list";
        public override string TypeDisplay => "List";
        public override Color TypeColor => new(120, 120, 120);

        public Type CollectionType => typeof(VariableValue);

        public IEnumerable<VariableValue> Values;

        public List() { }
        public List(IEnumerable<VariableValue> values)
        {
            Values = values;
        }

        protected override void SaveCustomData(BinaryWriter writer)
        {
            writer.Write((ushort)Values.Count());
            foreach (VariableValue value in Values)
                value.SaveData(writer);
        }
        protected override VariableValue LoadCustomData(BinaryReader reader)
        {
            VariableValue[] values = new VariableValue[reader.ReadInt16()];
            for (int i = 0; i < values.Length; i++)
                values[i] = LoadData(reader);

            return new List(values);
        }

        public IEnumerable<VariableValue> Enumerate() => Values;
    }
}
