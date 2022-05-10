using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerraIntegration.Components;
using TerraIntegration.Values;
using Terraria.ModLoader.IO;

namespace TerraIntegration.Variables
{
    public abstract class PropertyVariable : Variable
    {
        public static readonly Dictionary<string, Dictionary<string, PropertyVariable>> ByComponentType = new();
        public static readonly List<PropertyVariable> AllProperties = new();
        public static readonly List<PropertyVariable> WaitingComponent = new();

        public override string Type => $"{ComponentType}.{ComponentProperty}";

        public abstract string ComponentType { get; }
        public abstract string ComponentProperty { get; }

        public Point16 ComponentPos { get; set; }

        protected Component BoundComponentCache;
        public Component BoundComponent
        {
            get
            {
                if (BoundComponentCache is null)
                    Component.ByTypeName.TryGetValue(ComponentType, out BoundComponentCache);

                return BoundComponentCache;
            }
            set => BoundComponentCache = value;
        }

        public override string TypeDescription
        {
            get
            {
                string propd = PropertyDescription;

                string result = "";
                if (ComponentPos != default)
                {
                    result = $"[c/aaaa00:Bound to:] {BoundComponent?.Name ?? "Unregistered component"} at {ComponentPos.X}, {ComponentPos.Y}";
                }

                if (!propd.IsNullEmptyOrWhitespace())
                {
                    if (result.IsNullEmptyOrWhitespace()) result = propd;
                    else result += "\n" + propd;
                }

                return result;
            }
        }

        public virtual string PropertyDescription => "";

        public abstract VariableValue GetProperty(PositionedComponent c, HashSet<Error> errors);

        public virtual PropertyVariable CreateVariable(PositionedComponent c)
        {
            PropertyVariable pv = (PropertyVariable)Activator.CreateInstance(GetType());
            pv.ComponentPos = c.Pos;
            pv.BoundComponent = c.Component;

            return pv;
        }

        public override VariableValue GetValue(ComponentSystem system, HashSet<Error> errors)
        {
            Component c = system.GetComponent(ComponentPos, ComponentType, errors);
            if (c is null) return null;

            return GetProperty(new(ComponentPos, c), errors);
        }

        public static void ComponentRegistered()
        {
            List<PropertyVariable> success = new();

            foreach (PropertyVariable pv in WaitingComponent) 
            {
                if (pv.ComponentType is null) 
                    continue;

                Register(pv);
                success.Add(pv);
            }

            foreach (PropertyVariable pv in success)
                WaitingComponent.Remove(pv);
        }

        public static void Register(PropertyVariable property)
        {
            if (property.ComponentType is null)
            {
                WaitingComponent.Add(property);
                return;
            }

            if (!ByComponentType.TryGetValue(property.ComponentType, out var prop))
            {
                prop = new();
                ByComponentType[property.ComponentType] = prop;
            }
            prop[property.ComponentProperty] = property;
            Variable.ByTypeName[property.Type] = property;
        }

        public static void Unregister() 
        {
            ByComponentType.Clear();
            AllProperties.Clear();
        }

        protected override void SaveCustomData(BinaryWriter writer)
        {
            writer.Write(ComponentPos.X);
            writer.Write(ComponentPos.Y);
        }
        protected override Variable LoadCustomData(BinaryReader reader)
        {
            PropertyVariable pv = (PropertyVariable)Activator.CreateInstance(GetType());

            pv.ComponentPos = new(reader.ReadInt16(), reader.ReadInt16());
            return pv;
        }

        protected override object SaveCustomTag()
        {
            return new TagCompound()
            {
                ["comx"] = ComponentPos.X,
                ["comy"] = ComponentPos.Y,
            };
        }

        protected override Variable LoadCustomTag(object data)
        {
            PropertyVariable pv = (PropertyVariable)Activator.CreateInstance(GetType());


            if (data is TagCompound tag)
            {
                Point16 pos = default;

                if (tag.ContainsKey("comx"))
                    pos.X = tag.GetShort("comx");

                if (tag.ContainsKey("comy"))
                    pos.Y = tag.GetShort("comy");

                pv.ComponentPos = pos;
            }

            return pv;
        }
    }

    public abstract class PropertyVariable<TComponent> : PropertyVariable where TComponent : Component
    {
        public sealed override string ComponentType
        {
            get
            {
                if (Component.ByType.TryGetValue(typeof(TComponent), out Component instance))
                    BoundComponentCache = instance;

                return BoundComponentCache?.ComponentType;
            }
        }

        public abstract VariableValue GetProperty(TComponent component, Point16 pos, HashSet<Error> errors);
        public virtual PropertyVariable CreateVariable(TComponent component, Point16 pos) => (PropertyVariable)Activator.CreateInstance(GetType());

        public sealed override VariableValue GetProperty(PositionedComponent c, HashSet<Error> errors)
        {
            return GetProperty(c.Component as TComponent, ComponentPos, errors);
        }
    }
}
