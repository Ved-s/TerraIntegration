using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;

namespace TerraIntegration.Values
{
    public class VariableValue
    {
        public static readonly Dictionary<string, VariableValue> ByTypeName = new();
        public static readonly Dictionary<Type, VariableValue> ByType = new();

        public virtual Mod Mod => ModContent.GetInstance<TerraIntegration>();

        public virtual string Type => "any";
        public virtual string TypeDisplay => "Any";

        public virtual Color DisplayColor => Color.White;

        public virtual string Display() { return "null"; }

        public void SaveData(BinaryWriter writer)
        {
            if (this is UnloadedVariableValue unloaded)
            {
                writer.Write(unloaded.ValueType);
                writer.Write((ushort)unloaded.Data.Length);
                writer.Write(unloaded.Data);
                return;
            }

            writer.Write(Type);

            long lenPos = writer.BaseStream.Position;
            writer.Write((ushort)0);
            long startPos = writer.BaseStream.Position;
            SaveCustomData(writer);
            long endPos = writer.BaseStream.Position;
            long length = endPos - startPos;

            if (length == 0) return;
            writer.BaseStream.Seek(lenPos, SeekOrigin.Begin);
            writer.Write((ushort)length);
            writer.BaseStream.Seek(endPos, SeekOrigin.Begin);
        }
        public static VariableValue LoadData(BinaryReader reader)
        {
            string type = reader.ReadString();
            ushort length = reader.ReadUInt16();

            if (!ByTypeName.TryGetValue(type, out VariableValue value))
            {
                byte[] data = reader.ReadBytes(length);
                return new UnloadedVariableValue(type, data);
            }

            long pos = reader.BaseStream.Position;
            value = value.LoadCustomData(reader);
            long diff = (reader.BaseStream.Position - pos) - length;
            if (diff != 0)
            {
                if (diff > 0) value.Mod.Logger.WarnFormat("Variable {0} data overread: {1} bytes", type, diff);
                else value.Mod.Logger.WarnFormat("Variable {0} data underread: {1} bytes", type, -diff);

                reader.BaseStream.Seek(pos + length, SeekOrigin.Begin);
            }

            return value;
        }

        public virtual VariableValue GetFromCommand(CommandCaller caller, List<string> args) { return (VariableValue)Activator.CreateInstance(GetType()); }

        protected virtual void SaveCustomData(BinaryWriter writer) { }
        protected virtual VariableValue LoadCustomData(BinaryReader reader) { return (VariableValue)Activator.CreateInstance(GetType()); }

        public static string TypeToName(Type type, out Color color) 
        {
            if (type is null) 
            {
                color = Color.White;
                return null;
            }
            if (ByType.TryGetValue(type, out Values.VariableValue val))
            {
                color = val.DisplayColor;
                return val.TypeDisplay;
            }
            if (type.IsInterface)
            {
                string i = type.Name;
                if (i.StartsWith('I')) i = i[1..];

                color = new(0xaa, 0xbb, 0x00);
                return i;
            }
            color = new(0xff, 0xaa, 0xaa);
            return $"unregistered type {type.Name}";
        }
    }

    public class UnloadedVariableValue : VariableValue
    {
        public override string Type => "unloaded";
        public override string TypeDisplay => "Unloaded value";

        public override Color DisplayColor => Color.Red;
        public override string Display()
        {
            return "Unloaded";
        }

        public string ValueType { get; private set; }
        public byte[] Data { get; private set; }

        public UnloadedVariableValue() { }

        public UnloadedVariableValue(string type, byte[] data)
        {
            ValueType = type;
            Data = data;
        }
    }
}
