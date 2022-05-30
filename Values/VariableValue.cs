using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TerraIntegration.DisplayedValues;
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
        public virtual SpriteSheet DefaultSpriteSheet => null;
        public virtual SpriteSheetPos SpriteSheetPos => default;

        public virtual string Type => "any";
        public virtual string TypeDisplay => "Any";

        public virtual Color TypeColor => Color.White;

        public virtual DisplayedValue Display(ComponentSystem system) { return new ColorTextDisplay("", Color.White); }

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
                Mod m = value?.Mod ?? ModContent.GetInstance<TerraIntegration>();

                if (diff > 0) m.Logger.WarnFormat("Variable {0} data overread: {1} bytes", type, diff);
                else m.Logger.WarnFormat("Variable {0} data underread: {1} bytes", type, -diff);

                reader.BaseStream.Seek(pos + length, SeekOrigin.Begin);
            }

            return value;
        }

        public virtual VariableValue GetFromCommand(CommandCaller caller, List<string> args) { return (VariableValue)Activator.CreateInstance(GetType()); }

        protected virtual void SaveCustomData(BinaryWriter writer) { }
        protected virtual VariableValue LoadCustomData(BinaryReader reader) { return (VariableValue)Activator.CreateInstance(GetType()); }

        public static string TypeToName(Type type, bool colored)
        {
            if (type is null)
                return null;
            
            if (ByType.TryGetValue(type, out Values.VariableValue val))
            {
                if (colored)
                    return Util.ColorTag(val.TypeColor, val.TypeDisplay);

                return val.TypeDisplay;
            }
            if (type.IsInterface)
            {
                string name = type.Name;
                if (name.StartsWith('I'))
                    name = name[1..];

                if (type.IsGenericType)
                {
                    name = name.Split('`')[0];

                    string generics = string.Join(", ", type.GenericTypeArguments.Select(t => TypeToName(t, colored)));
                    if (colored)
                        return $"{Util.ColorTag(new(0xaa, 0xbb, 0x00), name)} of {generics}";
                    return $"{name} of {generics}";
                }

                if (colored)
                    return Util.ColorTag(new(0xaa, 0xbb, 0x00), name);
                return name;
            }

            if (colored) 
                return Util.ColorTag(new(0xff, 0xaa, 0xaa), $"unregistered type {type.Name}");
            return $"unregistered type {type.Name}";
        }
        public static string TypeToString(Type type)
        {
            if (type is null) return null;

            if (ByType.TryGetValue(type, out VariableValue val))
                return "V" + val.Type;
            
            else
                return "T" + type.FullName;
        }
        public static Type StringToType(string type) 
        {
            if (type is null) return null;

            bool istype = type.StartsWith('T');
            type = type[1..];

            if (istype)     
                return System.Type.GetType(type);
            
            else if (ByTypeName.TryGetValue(type, out VariableValue val))
                return val.GetType();
            
            return null;
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

        public static TValue GetInstance<TValue>() where TValue : VariableValue
        {
            if (ByType.TryGetValue(typeof(TValue), out VariableValue v))
                return v as TValue;
            return null;
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

        public virtual VariableValue Clone() => (VariableValue)MemberwiseClone();
    }

    public class UnloadedVariableValue : VariableValue
    {
        public override string Type => "unloaded";
        public override string TypeDisplay => "Unloaded value";

        public override Color TypeColor => Color.Red;
        public override DisplayedValue Display(ComponentSystem system) => new ColorTextDisplay("Unloaded", TypeColor);

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
