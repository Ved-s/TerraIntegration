using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using TerraIntegration.DataStructures;
using TerraIntegration.DisplayedValues;
using TerraIntegration.Interfaces;
using TerraIntegration.Templates;
using TerraIntegration.Variables;
using Terraria.ModLoader;

namespace TerraIntegration.Basic
{
    public abstract class VariableValue : ITypedObject
    {
        public readonly static SpriteSheet ValueSheet = new("TerraIntegration/Assets/Values/value", new(32, 32));
        public readonly static SpriteSheet ObjectSheet = new("TerraIntegration/Assets/Values/object", new(32, 32));

        public static readonly Dictionary<string, VariableValue> ByTypeName = new();
        public static readonly Dictionary<Type, VariableValue> ByType = new();

        public virtual Mod Mod => ModContent.GetInstance<TerraIntegration>();

        public virtual string Texture => null;
        public virtual SpriteSheet DefaultSpriteSheet => null;
        public virtual SpriteSheetPos SpriteSheetPos => default;

        public virtual bool HideInProgrammer => false;

        public virtual Color TypeColor => Color.White;

        public abstract string TypeName { get; }
        public string TypeDisplayName => Util.GetLangText(DisplayNameLocalizationKey, TypeDefaultDisplayName, DisplayNameFormatters);
        public string TypeDescription => Util.GetLangText(DescriptionLocalizationKey, TypeDefaultDescription, DescriptionFormatters);

        public abstract string TypeDefaultDisplayName { get; }
        public virtual string TypeDefaultDescription { get; }

        public virtual object[] DisplayNameFormatters { get; }
        public virtual object[] DescriptionFormatters { get; }

        public virtual string DescriptionLocalizationKey => "Mods.TerraIntegration.Descriptions.Values." + TypeName;
        public virtual string DisplayNameLocalizationKey => "Mods.TerraIntegration.Names.Values." + TypeName;

        public virtual DisplayedValue Display(ComponentSystem system) => null;

        public virtual bool ShouldDisplayReturnType(Variable var) => true;

        public static void SaveData(VariableValue value, BinaryWriter writer)
        {
            if (value is UnloadedVariableValue unloaded)
            {
                writer.Write(unloaded.ValueType);
                writer.Write((ushort)unloaded.Data.Length);
                writer.Write(unloaded.Data);
                return;
            }

            if (value is null)
            {
                writer.Write("");
                return;
            }

            writer.Write(value.TypeName);

            long lenPos = writer.BaseStream.Position;
            writer.Write((ushort)0);
            long startPos = writer.BaseStream.Position;
            value.SaveCustomData(writer);
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
            if (type == "") return null;

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

        public virtual VariableValue GetFromCommand(CommandCaller caller, List<string> args) { return NewInstance(); }

        protected virtual void SaveCustomData(BinaryWriter writer) { }
        protected virtual VariableValue LoadCustomData(BinaryReader reader) { return NewInstance(); }

        public virtual string FormatReturnType(ReturnType type, bool colored) => null;

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

        public virtual VariableValue Clone() => (VariableValue)MemberwiseClone();

        public virtual ReturnType GetReturnType() => GetType();

        public abstract bool Equals(VariableValue value);
        public override bool Equals(object obj)
        {
            return obj is VariableValue value
                && TypeName == value.TypeName
                && Equals(value);
        }

        public VariableValue NewInstance() => (VariableValue)Activator.CreateInstance(GetType());
        public static TValue GetInstance<TValue>() where TValue : VariableValue
        {
            if (ByType.TryGetValue(typeof(TValue), out VariableValue v))
                return v as TValue;
            return null;
        }

        public static string TypeToName<T>(bool colored = true) => TypeToName(typeof(T), colored);
        public static string TypeToName(Type type, bool colored)
        {
            if (type is null)
                return null;

            if (type == typeof(VariableValue))
                return "Any";

            if (ByType.TryGetValue(type, out VariableValue val))
            {
                if (colored)
                    return Util.ColorTag(val.TypeColor, val.TypeDisplayName);

                return val.TypeDefaultDisplayName;
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
                return "V" + val.TypeName;

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

        public virtual void OnRegister() { }

        public static void Register(VariableValue v)
        {
            if (v?.TypeName is null) return;

            ByTypeName[v.TypeName] = v;
            ByType[v.GetType()] = v;

            ValueProperty.ValueRegistered();

            v.OnRegister();
        }
        internal static void Unregister()
        {
            ByTypeName.Clear();
            ByType.Clear();
        }
    }

    public class UnloadedVariableValue : VariableValue
    {
        public override string TypeName => "unloaded";
        public override string TypeDefaultDisplayName => $"Unloaded value" + (ValueType.IsNullEmptyOrWhitespace() ? null : $" ({ValueType})");

        public override Color TypeColor => Color.Red;
        public override DisplayedValue Display(ComponentSystem system) => new ColorTextDisplay("Unloaded", TypeColor);

        public override bool HideInProgrammer => true;

        public string ValueType { get; private set; }
        public byte[] Data { get; private set; }

        public UnloadedVariableValue() { }

        public UnloadedVariableValue(string type, byte[] data)
        {
            ValueType = type;
            Data = data;
        }

        public override bool Equals(VariableValue value)
        {
            return value is UnloadedVariableValue unloaded
                && ValueType == unloaded.ValueType
                && Data == unloaded.Data;
        }
    }
}
