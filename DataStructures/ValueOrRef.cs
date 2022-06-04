using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerraIntegration.Basic;

namespace TerraIntegration.DataStructures
{
    public class ValueOrRef
    {
        public VariableValue Value;
        public Guid RefId;
        public bool IsRef;

        public ValueOrRef(VariableValue value) 
        {
            Value = value;
            IsRef = false;
        }

        public ValueOrRef(Guid refId) 
        {
            RefId = refId;
            IsRef = true;
        }

        public VariableValue GetValue(ComponentSystem system, List<Error> errors) 
        {
            if (IsRef) return system?.GetVariableValue(RefId, errors);
            return Value;
        }

        public void SaveData(BinaryWriter writer)
        {
            writer.Write(IsRef);
            if (IsRef) writer.Write(RefId.ToByteArray());
            else VariableValue.SaveData(Value, writer);
        }

        public static ValueOrRef LoadData(BinaryReader reader)
        {
            if (reader.ReadBoolean())
                return new(new Guid(reader.ReadBytes(16)));
            return new(VariableValue.LoadData(reader));

        }

        public override string ToString()
        {
            if (IsRef) return $"Ref {ComponentWorld.Instance.Guids.GetShortGuid(RefId)}";
            return Value?.Display(null)?.HoverText?.Replace('\n', ' ');
        }
    }
}
