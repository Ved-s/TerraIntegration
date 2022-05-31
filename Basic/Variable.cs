﻿using System;
using System.Collections.Generic;
using System.IO;
using TerraIntegration.DataStructures;
using TerraIntegration.Interfaces;
using TerraIntegration.Values;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace TerraIntegration.Basic
{
    public abstract class Variable
    {
        internal readonly static SpriteSheet BasicSheet = new("TerraIntegration/Assets/Types/basic", new(32, 32));
        internal readonly static SpriteSheet MathSheet = new("TerraIntegration/Assets/Types/math", new(32, 32));
        internal readonly static SpriteSheet CollectionSheet = new("TerraIntegration/Assets/Types/collection", new(32, 32));

        public virtual Mod Mod => ModContent.GetInstance<TerraIntegration>();
        public static ComponentWorld World => ModContent.GetInstance<ComponentWorld>();

        public static readonly Dictionary<Type, Variable> ByType = new();
        public static readonly Dictionary<string, Variable> ByTypeName = new();

        public virtual string Texture => null;
        public virtual SpriteSheet DefaultSpriteSheet => null;
        public virtual SpriteSheetPos SpriteSheetPos => default;

        public string Name { get; set; }
        public Guid Id { get; set; } = Guid.NewGuid();

        public string ShortId => ModContent.GetInstance<ComponentWorld>().Guids.GetShortGuid(Id);

        public abstract string Type { get; }
        public abstract string TypeDisplay { get; }

        public virtual string TypeDescription => null;

        public virtual Type VariableReturnType
        {
            get
            {
                return GetReturnTypeCache() ?? typeof(VariableValue);
            }
            set => SetReturnTypeCache(value);
        }

        public string ReturnTypeCacheName;
        public Type ReturnTypeCacheType;

        public abstract VariableValue GetValue(ComponentSystem system, List<Error> errors);
        public virtual Variable GetFromCommand(CommandCaller caller, List<string> args) => (Variable)Activator.CreateInstance(GetType());

        public static void SaveData(Variable var, BinaryWriter writer)
        {
            if (var is UnloadedVariable unloaded)
            {
                writer.Write(unloaded.UnloadedTypeName);
                writer.Write(var.Name ?? "");
                writer.Write((ushort)unloaded.UnloadedData.Length);
                writer.Write(var.Id.ToByteArray());
                writer.Write(unloaded.ReturnTypeCacheName ?? "");
                writer.Write(unloaded.UnloadedData);
                return;
            }

            if (var is null)
            {
                writer.Write("");
                return;
            }

            writer.Write(var.Type);
            writer.Write(var.Name ?? "");

            long lenPos = writer.BaseStream.Position;
            writer.Write((ushort)0);

            writer.Write(var.Id.ToByteArray());
            writer.Write(var.ReturnTypeCacheName ?? "");

            long startPos = writer.BaseStream.Position;
            var.SaveCustomData(writer);
            long endPos = writer.BaseStream.Position;
            long length = endPos - startPos;

            if (length == 0) return;
            writer.BaseStream.Seek(lenPos, SeekOrigin.Begin);
            writer.Write((ushort)length);
            writer.BaseStream.Seek(endPos, SeekOrigin.Begin);
        }
        public static Variable LoadData(BinaryReader reader)
        {
            string type = reader.ReadString();
            if (type == "") return null;
            string name = reader.ReadString();
            if (name.IsNullEmptyOrWhitespace())
                name = null;

            ushort length = reader.ReadUInt16();

            Guid id;
            string @return;

            if (!ByTypeName.TryGetValue(type, out Variable var))
            {
                id = new Guid(reader.ReadBytes(16));
                @return = reader.ReadString().NullIfEmpty();
                byte[] data = reader.ReadBytes(length);
                return new UnloadedVariable(type, data, null)
                {
                    Id = id,
                    Name = name,
                    ReturnTypeCacheName = @return,
                };
            }

            id = new Guid(reader.ReadBytes(16));
            @return = reader.ReadString().NullIfEmpty();

            long pos = reader.BaseStream.Position;
            var = var.LoadCustomData(reader);
            var.Id = id;
            var.Name = name;
            var.ReturnTypeCacheName = @return;
            World.Guids.AddToDictionary(id);

            long diff = (reader.BaseStream.Position - pos) - length;
            pos = reader.BaseStream.Position;
            if (diff != 0)
            {
                if (diff > 0) var.Mod.Logger.WarnFormat("Variable {0} data overread: {1} bytes", type, diff);
                else var.Mod.Logger.WarnFormat("Variable {0} data underread: {1} bytes", type, -diff);

                try
                {
                    reader.BaseStream.Seek(pos + length, SeekOrigin.Begin);
                }
                catch
                {
                    reader.BaseStream.Seek(pos, SeekOrigin.Begin);
                }
            }

            return var;
        }

        public static TagCompound SaveTag(Variable var)
        {
            TagCompound tag = new();

            if (var is null) return tag;

            if (var is UnloadedVariable unloaded)
            {
                tag["type"] = unloaded.UnloadedTypeName;
                tag["id"] = var.Id.ToByteArray();

                if (unloaded.UnloadedTag is not null)
                    tag["data"] = unloaded.UnloadedTag;

                if (unloaded.UnloadedData is not null)
                    tag["bytes"] = unloaded.UnloadedData;

                if (unloaded.Name is not null)
                    tag["name"] = unloaded.Name;

                if (unloaded.ReturnTypeCacheName is not null)
                    tag["return"] = unloaded.ReturnTypeCacheName;

                return tag;
            }

            tag["type"] = var.Type;

            if (var.Name is not null)
                tag["name"] = var.Name;

            tag["id"] = var.Id.ToByteArray();

            if (var.ReturnTypeCacheName is not null)
                tag["return"] = var.ReturnTypeCacheName;

            object custom = var.SaveCustomTag();
            if (custom is not null)
                tag["data"] = custom;

            MemoryStream stream = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(stream);

            var.SaveCustomData(bw);

            if (stream.Length > 0)
                tag["bytes"] = stream.ToArray();

            bw.Close();
            return tag;
        }
        public static Variable LoadTag(TagCompound tag)
        {
            if (!tag.ContainsKey("type")) return null;
            string type = tag.GetString("type");
            Guid id;

            string name = null;
            if (tag.ContainsKey("name"))
                name = tag.GetString("name");

            string @return = null;
            if (tag.ContainsKey("return"))
                @return = tag.GetString("return");

            if (!ByTypeName.TryGetValue(type, out Variable var))
            {
                object tagData = null;
                byte[] byteData = null;
                id = Guid.NewGuid();

                if (tag.ContainsKey("id")) id = new(tag.GetByteArray("id"));
                if (tag.ContainsKey("data")) tagData = tag["data"];
                if (tag.ContainsKey("bytes")) byteData = tag.GetByteArray("bytes");


                return new UnloadedVariable(type, byteData, tagData)
                {
                    Id = id,
                    Name = name,
                    ReturnTypeCacheName = @return
                };
            }

            Variable newVar = null;

            if (tag.ContainsKey("id"))
                id = new(tag.GetByteArray("id"));
            else id = Guid.NewGuid();

            if (tag.ContainsKey("data"))
                newVar = var.LoadCustomTag(tag["data"]);
            if (newVar is null && tag.ContainsKey("bytes"))
            {
                MemoryStream ms = new MemoryStream(tag.GetByteArray("bytes"));
                BinaryReader reader = new BinaryReader(ms);
                newVar = var.LoadCustomData(reader);
            }
            if (newVar is null) newVar = (Variable)Activator.CreateInstance(var.GetType());

            World.Guids.AddToDictionary(id);

            newVar.Id = id;
            newVar.Name = name;
            newVar.ReturnTypeCacheName = @return;

            return newVar;
        }

        protected virtual void SaveCustomData(BinaryWriter writer) { }
        protected virtual Variable LoadCustomData(BinaryReader reader) => (Variable)Activator.CreateInstance(GetType());

        protected virtual object SaveCustomTag() => null;
        protected virtual Variable LoadCustomTag(object data) => null;

        public virtual void HandlePacket(Point16 pos, ushort messageType, BinaryReader reader, int whoAmI, ref bool broadcast) { }
        public ModPacket CreatePacket(Point16 pos, ushort messageType) => Networking.CreateVariablePacket(Type, pos, messageType);

        public virtual void ModifyTooltips(List<TooltipLine> tooltips) { }

        public Variable Clone()
        {
            Variable var = CloneCustom();
            var.Name = Name;
            var.Id = Id;
            return var;
        }
        public virtual Variable CloneCustom() => (Variable)MemberwiseClone();

        public void SetReturnTypeCache(Type t)
        {
            if (t is null)
            {
                ReturnTypeCacheType = null;
                ReturnTypeCacheName = null;
            }
            else
            {
                ReturnTypeCacheType = t;
                ReturnTypeCacheName = VariableValue.TypeToString(t);
            }
        }
        public Type GetReturnTypeCache()
        {
            if (ReturnTypeCacheName is null && ReturnTypeCacheType is null)
                return null;

            if (ReturnTypeCacheType is not null)
                return ReturnTypeCacheType;

            ReturnTypeCacheType = VariableValue.StringToType(ReturnTypeCacheName);
            return ReturnTypeCacheType;
        }

        public static TVariable GetInstance<TVariable>() where TVariable : Variable
        {
            if (ByType.TryGetValue(typeof(TVariable), out Variable v))
                return v as TVariable;
            return null;
        }

        public static void Register(Variable v)
        {
            if (v is ComponentProperty pv)
            {
                ComponentProperty.Register(pv);
                return;
            }
            if (v is ValueProperty valpr)
            {
                ValueProperty.Register(valpr);
                return;
            }

            if (v?.Type is null) return;
            ByType[v.GetType()] = v;
            ByTypeName[v.Type] = v;
        }
        internal static void Unregister()
        {
            ByType.Clear();
            ByTypeName.Clear();

            ComponentProperty.Unregister();
            ValueProperty.Unregister();
        }

        public TValueInterface TryGetReturnTypeInterface<TValueInterface>() where TValueInterface : class, IValueInterface
        {
            if (VariableReturnType is not null
                && VariableReturnType.IsAssignableTo(typeof(TValueInterface))
                && VariableValue.ByType.TryGetValue(VariableReturnType, out VariableValue value))
                return value as TValueInterface;

            return null;
        }
        public TValue TryGetReturnType<TValue>() where TValue : VariableValue
        {
            if (VariableReturnType is not null
                && VariableReturnType == typeof(TValue)
                && VariableValue.ByType.TryGetValue(typeof(TValue), out VariableValue value))
                return (TValue)value;

            return null;
        }
    }

    public class UnloadedVariable : Variable
    {
        public override string Type => "unloaded";
        public override string TypeDisplay => $"Unloaded variable ({UnloadedTypeName})";

        public override SpriteSheetPos SpriteSheetPos => new(BasicSheet, 0, 0);

        public override Type VariableReturnType => typeof(UnloadedVariableValue);

        public string UnloadedTypeName { get; private set; }
        public byte[] UnloadedData { get; private set; }
        public object UnloadedTag { get; private set; }

        public UnloadedVariable() { }

        public UnloadedVariable(string type, byte[] data, object tag)
        {
            UnloadedTypeName = type;
            UnloadedData = data;
            UnloadedTag = tag;
        }

        public override VariableValue GetValue(ComponentSystem system, List<Error> errors)
        {
            return new UnloadedVariableValue();
        }
    }
}