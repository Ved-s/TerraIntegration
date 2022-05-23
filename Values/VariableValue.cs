using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerraIntegration.Variables;
using Terraria.ModLoader;

namespace TerraIntegration.Values
{
    public class VariableValue
    {
        public readonly static SpriteSheet BasicSheet = new("TerraIntegration/Assets/Values/basic", new(32, 32));

        public static readonly Dictionary<string, VariableValue> ByTypeName = new();
        public static readonly Dictionary<Type, VariableValue> ByType = new();

        public virtual Mod Mod => ModContent.GetInstance<TerraIntegration>();

        public virtual string Texture => null;
        public virtual SpriteSheet SpriteSheet => null;
        public virtual Point SpritesheetPos => default;

        public virtual string Type => "any";
        public virtual string TypeDisplay => "Any";

        public virtual Color TypeColor => Color.White;

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
                color = val.TypeColor;
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

        public IEnumerable<(Type, ValueProperty)> GetProperties() 
        {
            Type type = GetType();
            if (ValueProperty.ByValueType.TryGetValue(type, out var props))
                foreach (ValueProperty prop in props.Values)
                    yield return (type, prop);

            foreach (Type interf in type.GetInterfaces())
                if (ValueProperty.ByValueType.TryGetValue(interf, out var intprops))
                    foreach (ValueProperty prop in intprops.Values)
                        yield return (interf, prop);
        }
        public bool HasProperties()
        {
            Type type = GetType();
            if (ValueProperty.ByValueType.ContainsKey(type))
                return true;

            foreach (Type interf in type.GetInterfaces())
                if (ValueProperty.ByValueType.ContainsKey(interf))
                    return true;
                 
            return false;
        }

        public static void Register(VariableValue v)
        {
            if (v?.Type is null) return;

            ByTypeName[v.Type] = v;
            ByType[v.GetType()] = v;

            ValueProperty.ValueRegistered();
        }
        internal static void Unregister() 
        {
            ByTypeName.Clear();
            ByType.Clear();
        }
    }

    public class UnloadedVariableValue : VariableValue
    {
        public override string Type => "unloaded";
        public override string TypeDisplay => "Unloaded value";

        public override Color TypeColor => Color.Red;
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
