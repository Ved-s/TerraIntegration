using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TerraIntegration.ComponentProperties;
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
        public virtual string ComponentDisplayName { get; }

        public override string Texture
        {
            get
            {
                if (Mod.Name == nameof(TerraIntegration)) return $"{nameof(TerraIntegration)}/Assets/Tiles/{Name}";
                return base.Texture;
            }
        }
        public virtual ushort DefaultUpdateFrequency => 0;
        public virtual bool ConfigurableFrequency => true;

        public virtual bool CanHaveVariables => false;

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

        public bool InterfaceVisible => ModContent.GetInstance<ComponentInterface>().InterfaceComponent.Component?.ComponentType == ComponentType;
        public Point16 InterfacePos => ModContent.GetInstance<ComponentInterface>().InterfaceComponent.Pos;

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
            foreach (var var in data.Variables.Values)
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
            Variable v = data.GetVariable(varIndex);
            if (v is null) return;
            World.Guids.AddToDictionary(v.Id);
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
        public virtual bool ShouldSyncData(ComponentData data) => CanHaveVariables || DefaultUpdateFrequency > 0;

        public virtual IEnumerable<ComponentProperty> GetProperties()
        {
            if (!ComponentProperty.ByComponentType.TryGetValue(ComponentType, out var props))
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
            writer.Write((ushort)data.Variables.Values.Count(v => v is not null));

            foreach (var kvp in data.Variables)
            {
                if (kvp.Value is null) continue;
                writer.Write(kvp.Key);
                kvp.Value.Var.SaveData(writer);
            }
            writer.Write(data.UpdateFrequency);
            SendDataInternal(writer, data);
        }
        internal static ComponentData NetReceiveData(BinaryReader reader)
        {
            string component = reader.ReadString();
            Dictionary<int, Variable> vars = new();

            ushort count = reader.ReadUInt16();
            for (int i = 0; i < count; i++)
            {
                int index = reader.ReadInt16();
                Variables.Variable var = Variables.Variable.LoadData(reader);

                if (var is null) continue;

                vars[index] = var;
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

            foreach (var kvp in vars)
                data.SetVariable(kvp.Key, kvp.Value);

            data.UpdateFrequency = freq;

            return data;
        }

        internal virtual TagCompound SaveTag(ComponentData data)
        {
            TagCompound tag = new();

            tag["type"] = data is UnloadedComponentData u ? u.ComponentType : ComponentType;

            List<TagCompound> variables = new List<TagCompound>();

            foreach (var kvp in data.Variables)
            {
                if (kvp.Value is null) continue;
                
                TagCompound vartag = kvp.Value.Var.SaveTag();
                vartag["index"] = kvp.Key;
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

            Dictionary<int, Variable> vars = null;
            if (tag.ContainsKey("var"))
            {
                IList<TagCompound> variables = tag.GetList<TagCompound>("var");
                vars = new();

                int oldIndex = 0;

                foreach (TagCompound vartag in variables)
                {
                    int index;

                    if (!vartag.ContainsKey("index")) 
                        index = oldIndex;
                    else 
                        index = vartag.GetInt("index");

                    Variable var = Variable.LoadTag(vartag);

                    if (var is null) continue;

                    vars[index] = var;

                    do oldIndex++;
                    while (vars.ContainsKey(oldIndex));
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

            if (vars is not null)
                foreach (var kvp in vars)
                    data.SetVariable(kvp.Key, kvp.Value);

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
        public Dictionary<int, Items.Variable> Variables { get; internal set; }

        public void CopyTo(ComponentData data)
        {
            data.System = System;
            data.Variables = Variables;
            data.UpdateFrequency = UpdateFrequency;
        }

        internal void Init(Component c)
        {
            Component = c;
            Variables = new();
            UpdateFrequency = c.DefaultUpdateFrequency;
            CustomInit(c);
        }

        public virtual void CustomInit(Component c) { }

        internal void Destroy(Point16 pos)
        {
            foreach (Items.Variable v in Variables.Values)
                if (v is not null)
                {
                    Util.DropItemInWorld(v.Item, pos.X * 16, pos.Y * 16);
                }
            Variables.Clear();
        }

        public Variable GetVariable(int index) 
        {
            if (Variables.TryGetValue(index, out Items.Variable var))
                return var?.Var;
            return null;
        }
        public Items.Variable GetVariableItem(int index)
        {
            if (Variables.TryGetValue(index, out Items.Variable var))
                return var;
            return null;
        }
        public void SetVariable(int index, Variable var)
        {
            Items.Variable v = Util.CreateModItem<Items.Variable>();
            v.Var = var;
            Variables[index] = v;
        }
        public void SetVariable(int index, Items.Variable var)
        {
            Variables[index] = var;
        }
        public void ClearVariable(int index)
        {
            Variables.Remove(index);
        }
        public bool HasVariable(int index)
        {
            return Variables.ContainsKey(index) && Variables[index] is not null;
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
