using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerraIntegration.Components;
using TerraIntegration.Values;
using TerraIntegration.Variables;
using Terraria.ModLoader.IO;

namespace TerraIntegration.Variables
{
    public abstract class ComponentProperty : Variable
    {
        public static readonly Dictionary<string, Dictionary<string, ComponentProperty>> ByComponentType = new();
        public static readonly List<ComponentProperty> AllProperties = new();
        public static readonly List<ComponentProperty> WaitingComponent = new();

        public override string Type => $"{ComponentType}.{PropertyName}";

        public abstract string ComponentType { get; }
        public abstract string PropertyName { get; }

        public override SpriteSheet DefaultSpriteSheet
        {
            get
            {
                if (Component.ByTypeName.TryGetValue(ComponentType, out Component com))
                    return com.DefaultPropertySpriteSheet;

                return null;
            }
        }

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

        public abstract VariableValue GetProperty(PositionedComponent c, List<Error> errors);

        public virtual ComponentProperty CreateVariable(PositionedComponent c)
        {
            ComponentProperty pv = (ComponentProperty)Activator.CreateInstance(GetType());
            pv.ComponentPos = c.Pos;
            pv.BoundComponent = c.Component;

            return pv;
        }

        public override VariableValue GetValue(ComponentSystem system, List<Error> errors)
        {
            Component c = system.GetComponent(ComponentPos, ComponentType, errors);
            if (c is null) return null;

            return GetProperty(new(ComponentPos, c), errors);
        }

        protected override void SaveCustomData(BinaryWriter writer)
        {
            writer.Write(ComponentPos.X);
            writer.Write(ComponentPos.Y);
        }
        protected override Variable LoadCustomData(BinaryReader reader)
        {
            ComponentProperty pv = (ComponentProperty)Activator.CreateInstance(GetType());

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
            ComponentProperty pv = (ComponentProperty)Activator.CreateInstance(GetType());


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

        internal static void ComponentRegistered()
        {
            List<ComponentProperty> success = new();

            foreach (ComponentProperty pv in WaitingComponent)
            {
                if (pv.ComponentType is null)
                    continue;

                Register(pv);
                success.Add(pv);
            }

            foreach (ComponentProperty pv in success)
                WaitingComponent.Remove(pv);
        }
        public static void Register(ComponentProperty property)
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
            prop[property.PropertyName] = property;
            Variable.ByTypeName[property.Type] = property;
        }
        public static new void Unregister()
        {
            ByComponentType.Clear();
            AllProperties.Clear();
        }
    }

    public abstract class ComponentProperty<TComponent> : ComponentProperty where TComponent : Component
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

        public override SpriteSheet DefaultSpriteSheet
        {
            get
            {
                if (Component.ByType.TryGetValue(typeof(TComponent), out Component com))
                    return com.DefaultPropertySpriteSheet;

                return null;
            }
        }

        public abstract VariableValue GetProperty(TComponent component, Point16 pos, List<Error> errors);
        public virtual ComponentProperty CreateVariable(TComponent component, Point16 pos) => (ComponentProperty)Activator.CreateInstance(GetType());

        public sealed override VariableValue GetProperty(PositionedComponent c, List<Error> errors)
        {
            return GetProperty(c.Component as TComponent, ComponentPos, errors);
        }
    }
}
