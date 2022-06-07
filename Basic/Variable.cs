using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TerraIntegration.DataStructures;
using TerraIntegration.Interfaces;
using TerraIntegration.Templates;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace TerraIntegration.Basic
{
    public abstract class Variable : ITypedObject
    {
        internal readonly static SpriteSheet BasicSheet = new("TerraIntegration/Assets/Types/basic", new(32, 32));
        internal readonly static SpriteSheet MathSheet = new("TerraIntegration/Assets/Types/math", new(32, 32));
        internal readonly static SpriteSheet CollectionSheet = new("TerraIntegration/Assets/Types/collection", new(32, 32));
        internal readonly static SpriteSheet BooleanSheet = new("TerraIntegration/Assets/Types/boolean", new(32, 32));
        internal readonly static SpriteSheet ComparingSheet = new("TerraIntegration/Assets/Types/comparing", new(32, 32));
        internal readonly static SpriteSheet TileSheet = new("TerraIntegration/Assets/Types/tile", new(32, 32));

        public virtual Mod Mod => ModContent.GetInstance<TerraIntegration>();
        public static ComponentWorld World => ModContent.GetInstance<ComponentWorld>();

        public static readonly Dictionary<Type, Variable> ByType = new();
        public static readonly Dictionary<string, Variable> ByTypeName = new();

        public virtual string Texture => null;
        public virtual SpriteSheet DefaultSpriteSheet => null;
        public virtual SpriteSheetPos SpriteSheetPos => default;

        public string Name { get; set; }
        public Guid Id { get; set; } = Guid.NewGuid();

        public TypeIdentity TypeIdentity => TypeIdentity.Variable(this);

        public string ShortId => ModContent.GetInstance<ComponentWorld>().Guids.GetShortGuid(Id);

        public abstract string TypeName { get; }
        public string TypeDisplayName => Util.GetLangTextOrNull(DisplayNameLocalizationKey) ?? TypeDefaultDisplayName;
        public string TypeDescription => Util.GetLangTextOrNull(DescriptionLocalizationKey) ?? TypeDefaultDescription;
        public string ItemName => Name ?? Util.GetLangTextOrNull(ItemNameLocalizationKey) ?? DefaultItemName;

        public virtual string DefaultItemName => "Variable";
        public abstract string TypeDefaultDisplayName { get; }
        public virtual string TypeDefaultDescription { get; }

        public virtual string DescriptionLocalizationKey => "Mods.TerraIntegration.Descriptions.Variables." + TypeName;
        public virtual string DisplayNameLocalizationKey => "Mods.TerraIntegration.Names.Variables." + TypeName;
        public virtual string ItemNameLocalizationKey => "Mods.TerraIntegration.ItemNames.Variable";
        
        public virtual Type VariableReturnType
        {
            get
            {
                return GetReturnTypeCache() ?? typeof(VariableValue);
            }
            set => SetReturnTypeCache(value);
        }

        public virtual Type[] RelatedTypes => null;
        public virtual bool VisibleInProgrammerVariables => true;
        public virtual bool ShowLastValue => true;

        public string ReturnTypeCacheName;
        public Type ReturnTypeCacheType;
        public VariableValue LastValue;
        public ComponentSystem LastSystem;

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

            writer.Write(var.TypeName);
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

            tag["type"] = var.TypeName;

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
                newVar = var.LoadCustomTag(tag.GetCompound("data"));
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

        protected virtual TagCompound SaveCustomTag() => null;
        protected virtual Variable LoadCustomTag(TagCompound data) => null;

        public virtual void HandlePacket(Point16 pos, ushort messageType, BinaryReader reader, int whoAmI, ref bool broadcast) { }
        public ModPacket CreatePacket(Point16 pos, ushort messageType) => Networking.CreateVariablePacket(TypeName, pos, messageType);

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

        public void SetLastValue(VariableValue value, ComponentSystem system)
        {
            if (!ShowLastValue) return;
            LastValue = value;
            LastSystem = system;
        }

        public static TVariable GetInstance<TVariable>() where TVariable : Variable
        {
            if (ByType.TryGetValue(typeof(TVariable), out Variable v))
                return v as TVariable;
            return null;
        }

        public static void Register(Variable v, bool skipSubtypeChecks = false)
        {
            if (!skipSubtypeChecks)
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
            }
            if (v?.TypeName is null) return;
            ByType[v.GetType()] = v;
            ByTypeName[v.TypeName] = v;
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

        public static IEnumerable<(Type, Variable)> GetRelated(IEnumerable<Type> types)
        {
            HashSet<Type> allTypes = new(types);
            allTypes.UnionWith(types.SelectMany(t => t.GetInterfaces().Where(i => i.GetInterfaces().Any(t => t == typeof(IValueInterface)))));

            foreach (Variable var in ByType.Values)
            {
                var rel = var.RelatedTypes;
                if (rel is null) continue;
                foreach (Type relType in rel)
                    if (allTypes.Contains(relType))
                        yield return (relType, var);
            }
        }

        protected bool TryGetValueType<TValue>(VariableValue value, List<Error> errors, out TValue newValue) where TValue : VariableValue
        {
            newValue = null;
            if (CheckValueType(value, typeof(TValue), errors))
            {
                newValue = value as TValue;
                return true;
            }
            return false;
        }

        protected bool CheckValueType(VariableValue value, Type type, List<Error> errors)
        {
            if (value is null && errors.Count > 0) return false;

            if (value is null || !value.GetType().IsAssignableTo(type))
            {
                errors.Add(Errors.ExpectedValue(type, TypeIdentity));
                return false;
            }

            return true;
        }
    }

    public class UnloadedVariable : Variable
    {
        public override string TypeName => "unloaded";
        public override string TypeDefaultDisplayName => $"Unloaded variable ({UnloadedTypeName})";

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
