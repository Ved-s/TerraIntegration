using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using TerraIntegration.UI;
using TerraIntegration.Variables;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.ObjectData;

namespace TerraIntegration.Components
{
    public abstract class Component : ModTile
    {
        

        public static HashSet<int> TileTypes = new();
        public static Dictionary<string, Component> ByTypeName = new();
        public static Dictionary<int, Component> ByTileType = new();
        public static Dictionary<Type, Component> ByType = new();

        private UIPanel @interface;

        public new static TerraIntegration Mod => ModContent.GetInstance<TerraIntegration>();
        public static ComponentWorld World => ModContent.GetInstance<ComponentWorld>();

        public abstract string ComponentType { get; }
        public override string Texture
        {
            get
            {
                if (Mod.Name == nameof(TerraIntegration)) return $"{nameof(TerraIntegration)}/Assets/Tiles/{Name}";
                return base.Texture;
            }
        }
        public virtual ushort DefaultUpdateFrequency => 0;
        public virtual int VariableSlots => 0;

        public virtual string DefaultPropertyTexture => null;
        public virtual SpriteSheet DefaultPropertySpriteSheet => null;
        public virtual Point DefaultPropertySpriteSheetPos => default;

        public virtual bool HasRightClickInterface => false;
        public virtual Vector2 InterfaceOffset { get; protected set; }
        public UIPanel Interface
        {
            get
            {
                SetupInterfaceIfNeeded();
                return @interface;
            }
        }

        internal virtual bool HasData => false;

        public override bool RightClick(int i, int j)
        {
            if (HasRightClickInterface)
            {
                Point16 pos = new(i, j);
                pos = GetInterfaceTarget(pos);
                ModContent.GetInstance<ComponentInterface>().SetInterfaceComponent(pos, this);
                return true;
            }
            return false;
        }

        public void SetupNewTile()
        {
            TileObjectData.newTile.Width = 1;
            TileObjectData.newTile.Height = 1;
            TileObjectData.newTile.Origin = new(0, 0);
            TileObjectData.newTile.CoordinateHeights = new[] { 16 };
            TileObjectData.newTile.CoordinateWidth = 16;
            TileObjectData.newTile.CoordinatePadding = 2;
            TileObjectData.newTile.HookCheckIfCanPlace = new PlacementHook((_, _, _, _, _, _) => 0, -1, 0, true);
            TileObjectData.newTile.UsesCustomCanPlace = true;
            //TileObjectData.newTile.HookPostPlaceEveryone = new PlacementHook((x, y, _, _, _, _) => { OnPlaced(new(x, y)); return 0; }, -1, 0, true);
        }

        public void SetupInterfaceIfNeeded()
        {
            if (@interface is null && HasRightClickInterface)
                @interface = SetupInterface();
        }

        [CallSide(CallSide.Both)]
        public virtual void OnPlaced(Point16 pos)
        {
            InitData(pos);
            ComponentSystem.UpdateSystem(pos);
            if (DefaultUpdateFrequency > 0)
                World.ComponentUpdates[pos] = this;
        }
        [CallSide(CallSide.Both)]
        public virtual void OnKilled(Point16 pos)
        {
            ComponentSystem.UpdateSystem(pos);
            World.RemoveAll(pos);
        }
        [CallSide(CallSide.Both)]
        public virtual void OnLoaded(Point16 pos)
        {
            if (DefaultUpdateFrequency > 0)
                World.ComponentUpdates[pos] = this;

            ComponentData data = GetData(pos);
            foreach (var var in data.Variables)
                if (var is not null)
                    World.Guids.AddToDictionary(var.Var.Id);
        }
        [CallSide(CallSide.Both)]
        public virtual void OnUpdate(Point16 pos) { }
        [CallSide(CallSide.Server)]
        public virtual void OnEvent(Point16 pos, int variableIndex) { }
        [CallSide(CallSide.Server)]
        public virtual void OnSystemUpdate(Point16 pos) { }
        [CallSide(CallSide.Both)]
        public virtual void OnVariableChanged(Point16 pos, int varIndex)
        {
            ComponentData data = GetData(pos);
            Networking.SendComponentVariable(pos, varIndex);
            if (data.Variables[varIndex] is null) return;
            World.Guids.AddToDictionary(data.Variables[varIndex].Var.Id);
        }
        [CallSide(CallSide.Both)]
        public virtual void OnPlayerJoined(int player) { }

        public virtual string GetHoverText(Point16 pos) => null;
        public virtual UIPanel SetupInterface() { return new(); }
        public virtual void UpdateInterface(Point16 pos) { }
        public virtual Point16 GetInterfaceTarget(Point16 pos) => pos;
        public virtual bool CheckShowInterface(Point16 pos) => true;
        public virtual Vector2 GetInterfaceReachCheckPos(Point16 pos)
        {
            return pos.ToVector2() * 16 + new Vector2(8);
        }

        public virtual bool ShouldSaveData(ComponentData data) => true;
        public virtual bool ShouldSyncData(ComponentData data) => VariableSlots > 0 || DefaultUpdateFrequency > 0;

        public virtual IEnumerable<PropertyVariable> GetProperties()
        {
            if (!PropertyVariable.ByComponentType.TryGetValue(ComponentType, out var props))
                return null;

            return props.Values;
        }

        public void ReloadInterface()
        {
            if (!HasRightClickInterface) return;
            @interface = SetupInterface();
        }

        public void SetUpdates(Point16 pos, bool updates)
        {
            if (updates) World.ComponentUpdates[pos] = this;
            else World.ComponentUpdates.Remove(pos);
        }
        public void SetUpdates(Point16 pos, ushort rate)
        {
            ComponentData data = GetData(pos);
            data.UpdateFrequency = rate;

            Networking.SendComponentFrequency(pos);

            SetUpdates(pos, rate > 0);
        }

        public virtual bool HandlePacket(Point16 pos, ushort messageType, BinaryReader reader, int whoAmI, ref bool broadcast) { return false; }
        public ModPacket CreatePacket(Point16 pos, ushort messageType) => Networking.CreateComponentPacket(ComponentType, pos, messageType);

        public ComponentData GetData(Point16 pos) => ModContent.GetInstance<ComponentWorld>().GetData(pos, this);
        public ComponentData GetDataOrNull(Point16 pos) => ModContent.GetInstance<ComponentWorld>().GetDataOrNull(pos);

        internal virtual void NetSendData(BinaryWriter writer, ComponentData data)
        {
            if (data is UnloadedComponentData)
                throw new InvalidDataException("Unloaded data should not be synced");

            writer.Write(ComponentType);
            writer.Write((ushort)data.Variables.Length);

            foreach (Items.Variable var in data.Variables)
            {
                if (var is null)
                {
                    writer.Write("");
                    continue;
                }

                var.Var.SaveData(writer);
            }
            writer.Write(data.UpdateFrequency);
            SendDataInternal(writer, data);
        }
        internal static ComponentData NetReceiveData(BinaryReader reader)
        {
            string component = reader.ReadString();
            Items.Variable[] vars = new Items.Variable[reader.ReadUInt16()];

            for (int i = 0; i < vars.Length; i++)
            {
                Variables.Variable var = Variables.Variable.LoadData(reader);

                if (var is null) continue;

                Item item = new();
                item.SetDefaults(ModContent.ItemType<Items.Variable>());
                Items.Variable itemvar = item.ModItem as Items.Variable;
                itemvar.Var = var;
                vars[i] = itemvar;
            }
            ushort freq = reader.ReadUInt16();
            ushort dataLength = reader.ReadUInt16();

            ComponentData data;
            if (!ByTypeName.TryGetValue(component, out Component c))
            {
                if (dataLength > 0)
                    reader.ReadBytes(dataLength);
                return null;
            }

            data = c.ReceiveDataInternal(reader, dataLength, component);
            data.Init(c);

            for (int i = 0; i < vars.Length; i++)
            {
                if (i >= data.Variables.Length) break;
                data.Variables[i] = vars[i];
            }
            data.UpdateFrequency = freq;

            return data;
        }

        internal virtual TagCompound SaveTag(ComponentData data)
        {
            TagCompound tag = new();

            tag["type"] = data is UnloadedComponentData u ? u.ComponentType : ComponentType;

            List<TagCompound> variables = new List<TagCompound>();

            foreach (Items.Variable var in data.Variables)
            {
                if (var is null)
                {
                    variables.Add(new());
                    continue;
                }
                TagCompound vartag = var.Var.SaveTag();
                variables.Add(vartag);
            }
            tag["var"] = variables;
            if (DefaultUpdateFrequency > 0)
                tag["freq"] = data.UpdateFrequency;
            object @internal = SaveTagInternal(data);
            if (@internal is not null)
            {
                tag["data"] = @internal;
            }
            return tag;
        }
        internal static ComponentData LoadTag(TagCompound tag)
        {
            if (!tag.ContainsKey("type")) return null;
            string component = tag.GetString("type");

            List<Items.Variable> vars = null;
            if (tag.ContainsKey("var"))
            {
                IList<TagCompound> variables = tag.GetList<TagCompound>("var");
                vars = new();

                foreach (TagCompound vartag in variables)
                {
                    Variables.Variable var = Variables.Variable.LoadTag(vartag);

                    if (var is null)
                    {
                        vars.Add(null);
                        continue;
                    }

                    Item item = new();
                    item.SetDefaults(ModContent.ItemType<Items.Variable>());
                    Items.Variable itemvar = item.ModItem as Items.Variable;
                    itemvar.Var = var;
                    vars.Add(itemvar);
                }
            }

            object @internal = null;
            if (tag.ContainsKey("data"))
            {
                @internal = tag["data"];
            }

            ComponentData data;
            if (!ByTypeName.TryGetValue(component, out Component c))
                data = new UnloadedComponentData(component, @internal);
            else
            {
                data = c.LoadTagInternal(@internal);
                data.Init(c);
            }

            if (tag.ContainsKey("freq"))
            {
                data.UpdateFrequency = (ushort)tag.GetShort("freq");
            }

            for (int i = 0; i < vars.Count; i++)
            {
                if (i >= data.Variables.Length) break;
                data.Variables[i] = vars[i];
            }

            return data;
        }

        internal virtual ComponentData ReceiveDataInternal(BinaryReader reader, ushort dataLength, string type) { return new(); }
        internal virtual void SendDataInternal(BinaryWriter writer, ComponentData data)
        {
            writer.Write((ushort)0);
        }

        internal virtual object SaveTagInternal(ComponentData data) => null;
        internal virtual ComponentData LoadTagInternal(object tag) => new();

        internal virtual void InitData(Point16 pos) 
        {
            World.InitData(pos, this);
        }

    }

    public class ComponentData
    {
        public Component Component { get; internal set; }
        public ComponentSystem System { get; internal set; }

        public ushort UpdateFrequency { get; set; } = 1;
        public Items.Variable[] Variables { get; internal set; }

        public void CopyTo(ComponentData data)
        {
            data.System = System;
            data.Variables = Variables;
            data.UpdateFrequency = UpdateFrequency;
        }

        internal void Init(Component c)
        {
            Component = c;
            Variables = new Items.Variable[c.VariableSlots];
            UpdateFrequency = c.DefaultUpdateFrequency;
            CustomInit(c);
        }

        public virtual void CustomInit(Component c) { }

        internal void Destroy(Point16 pos)
        {
            for (int i = 0; i < Variables.Length; i++)
                if (Variables[i] is not null)
                {
                    Util.DropItemInWorld(Variables[i].Item, pos.X * 16, pos.Y * 16);
                    Variables[i] = null;
                }
        }
    }

    public abstract class Component<TDataType> : Component where TDataType : ComponentData, new()
    {
        internal override bool HasData => true;

        public new TDataType GetData(Point16 pos) => ModContent.GetInstance<ComponentWorld>().GetData<TDataType>(pos, this);
        public new TDataType GetDataOrNull(Point16 pos) => ModContent.GetInstance<ComponentWorld>().GetDataOrNull<TDataType>(pos);

        public virtual object SaveCustomDataTag(TDataType data) => null;
        public virtual TDataType LoadCustomDataTag(object data) => new();

        public virtual void SendCustomData(TDataType data, BinaryWriter writer) { }
        public virtual TDataType ReceiveCustomData(BinaryReader reader) => new();

        internal override object SaveTagInternal(ComponentData data)
        {
            if (data is not TDataType tdata)
            {
                return null;
            }
            return SaveCustomDataTag(tdata);
        }
        internal override ComponentData LoadTagInternal(object tag)
        {
            return LoadCustomDataTag(tag);
        }

        internal override void SendDataInternal(BinaryWriter writer, ComponentData data)
        {
            if (data is not TDataType tdata)
            {
                writer.Write((ushort)0);
                return;
            }

            long lenPos = writer.BaseStream.Position;
            writer.Write((ushort)0);
            long startPos = writer.BaseStream.Position;
            SendCustomData(tdata, writer);
            long endPos = writer.BaseStream.Position;
            long length = endPos - startPos;

            if (length == 0) return;
            writer.BaseStream.Seek(lenPos, SeekOrigin.Begin);
            writer.Write((ushort)length);
            writer.BaseStream.Seek(endPos, SeekOrigin.Begin);
        }
        internal override ComponentData ReceiveDataInternal(BinaryReader reader, ushort length, string type)
        {
            long pos = reader.BaseStream.Position;
            ComponentData data = ReceiveCustomData(reader);
            long diff = (reader.BaseStream.Position - pos) - length;
            if (diff != 0)
            {
                if (diff > 0) Mod.Logger.WarnFormat("Component {0} data overread: {1} bytes", type, diff);
                else Mod.Logger.WarnFormat("Component {0} data underread: {1} bytes", type, -diff);

                reader.BaseStream.Seek(pos + length, SeekOrigin.Begin);
            }
            return data;
        }

        public override bool ShouldSaveData(ComponentData data)
        {
            if (data is TDataType) return ShouldSaveData((TDataType)data);
            return true;
        }
        public virtual bool ShouldSaveData(TDataType data) => true;

        internal override void InitData(Point16 pos)
        {
            World.InitData(pos, this);
        }
    }

    public class UnloadedComponentData : ComponentData
    {
        public string ComponentType { get; private set; }
        public object Data { get; private set; }

        public UnloadedComponentData(string component, object data)
        {
            ComponentType = component;
            Data = data;
        }

        public void SaveTag()
        {
            throw new NotImplementedException();
        }
    }

    [Autoload(false)]
    public class UnloadedComponent : Component
    {
        public override string ComponentType => null;
    }
}
