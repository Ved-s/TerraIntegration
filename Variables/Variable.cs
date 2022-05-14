using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerraIntegration.Values;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace TerraIntegration.Variables
{
    public class Variable
    {
        public virtual Mod Mod => ModContent.GetInstance<TerraIntegration>();
        public static ComponentWorld World => ModContent.GetInstance<ComponentWorld>();

        public static readonly Dictionary<string, Variable> ByTypeName = new();

        private Item item;

        public Guid Id { get; set; }
        public string ShortId => ModContent.GetInstance<ComponentWorld>().Guids.GetShortGuid(Id);

        public bool IsEmpty => GetType() == typeof(Variable);

        public virtual string Type => "any";
        public virtual string TypeDisplay => "Any";

        public virtual string TypeDescription => null;

        public virtual Type VariableReturnType => typeof(VariableValue);

        public Item Item
        {
            get
            {
                if (item is null || item.IsAir)
                {
                    item = new();
                    item.SetDefaults(ModContent.ItemType<Items.Variable>());

                    Items.Variable var = item.ModItem as Items.Variable;
                    var.Var = this;
                }
                return item;
            }
            internal set { item = value; }
        }

        public Variable()
        {
            if (!IsEmpty)
                Id = Guid.NewGuid();
        }

        public virtual VariableValue GetValue(ComponentSystem system, List<Error> errors)
        {
            return new();
        }
        public virtual Variable GetFromCommand(CommandCaller caller, List<string> args) => (Variable)Activator.CreateInstance(GetType());

        public void SaveData(BinaryWriter writer) 
        {
            if (this is UnloadedVariable unloaded)
            {
                writer.Write(unloaded.UnloadedTypeName);
                writer.Write((ushort)unloaded.UnloadedData.Length);
                writer.Write(Id.ToByteArray());
                writer.Write(unloaded.UnloadedData);
                return;
            }

            writer.Write(Type);

            if (IsEmpty)
            {
                writer.Write((ushort)0);
                return;
            }

            long lenPos = writer.BaseStream.Position;
            writer.Write((ushort)0);

            writer.Write(Id.ToByteArray());
            
            long startPos = writer.BaseStream.Position;
            SaveCustomData(writer);
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

            ushort length = reader.ReadUInt16();

            Guid id;

            if (!ByTypeName.TryGetValue(type, out Variable var))
            {
                id = new Guid(reader.ReadBytes(16));
                byte[] data = reader.ReadBytes(length);
                return new UnloadedVariable(type, data, null) { Id = id };
            }

            if (var.IsEmpty) return var;

            id = new Guid(reader.ReadBytes(16));

            long pos = reader.BaseStream.Position;
            var = var.LoadCustomData(reader);
            var.Id = id;
            World.Guids.AddToDictionary(id);

            long diff = (reader.BaseStream.Position - pos) - length;
            pos = reader.BaseStream.Position;
            if (diff != 0)
            {
                if (diff > 0) var.Mod.Logger.WarnFormat("Variable {0} data overread: {1} bytes", type, diff);
                else  var.Mod.Logger.WarnFormat("Variable {0} data underread: {1} bytes", type, -diff);

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

        public TagCompound SaveTag()
        {
            TagCompound tag = new();

            if (this is UnloadedVariable unloaded) 
            {
                tag["type"] = unloaded.UnloadedTypeName;
                tag["id"] = Id.ToByteArray();

                if (unloaded.UnloadedTag is not null)
                    tag["data"] = unloaded.UnloadedTag;

                if (unloaded.UnloadedData is not null)
                    tag["bytes"] = unloaded.UnloadedData;

                return tag;
            }

            tag["type"] = Type;
            if (IsEmpty) return tag;

            tag["id"] = Id.ToByteArray();

            object custom = SaveCustomTag();
            if (custom is not null)
                tag["data"] = custom;

            MemoryStream stream = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(stream);

            SaveCustomData(bw);

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

            if (!ByTypeName.TryGetValue(type, out Variable var))
            {
                object tagData = null;
                byte[] byteData = null;
                id = Guid.NewGuid();

                if (tag.ContainsKey("id")) id = new(tag.GetByteArray("id"));
                if (tag.ContainsKey("data")) tagData = tag["data"];
                if (tag.ContainsKey("bytes")) byteData = tag.GetByteArray("bytes");


                return new UnloadedVariable(type, byteData, tagData) { Id = id };
            }

            Variable newVar = null;

            if (tag.ContainsKey("id")) 
                id = new(tag.GetByteArray("id"));
            else id = Guid.NewGuid();

            if (tag.ContainsKey("data"))
                newVar = var.LoadCustomTag(tag["data"]);
            else if (tag.ContainsKey("bytes"))
            {
                MemoryStream ms = new MemoryStream(tag.GetByteArray("bytes"));
                BinaryReader reader = new BinaryReader(ms);
                newVar = var.LoadCustomData(reader);
            } 
            else newVar = (Variable)Activator.CreateInstance(var.GetType());

            World.Guids.AddToDictionary(id);

            newVar.Id = id;
            return newVar;
        }

        protected virtual void SaveCustomData(BinaryWriter writer) { }
        protected virtual Variable LoadCustomData(BinaryReader reader) => (Variable)Activator.CreateInstance(GetType());

        protected virtual object SaveCustomTag() => null;
        protected virtual Variable LoadCustomTag(object data) => (Variable)Activator.CreateInstance(GetType());

        public virtual void HandlePacket(Point16 pos, ushort messageType, BinaryReader reader, int whoAmI, ref bool broadcast) { }
        public ModPacket CreatePacket(Point16 pos, ushort messageType) => Networking.CreateVariablePacket(Type, pos, messageType);

        public virtual void ModifyTooltips(List<TooltipLine> tooltips) { }
    }

    public class UnloadedVariable : Variable
    {
        public override string Type => "unloaded";
        public override string TypeDisplay => $"Unloaded variable ({UnloadedTypeName})";

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
