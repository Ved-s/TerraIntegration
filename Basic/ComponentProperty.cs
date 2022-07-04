using System;
using System.Collections.Generic;
using System.IO;
using TerraIntegration.Components;
using TerraIntegration.DataStructures;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace TerraIntegration.Basic
{
    public abstract class ComponentProperty : Variable
    {
        public static readonly Dictionary<string, Dictionary<string, ComponentProperty>> ByComponentType = new();
        public static readonly List<ComponentProperty> AllProperties = new();
        public static readonly List<ComponentProperty> WaitingComponent = new();

        public override string TypeName => $"{ComponentType}.{PropertyName}";

        public abstract string ComponentType { get; }
        public abstract string PropertyName { get; }
        public abstract string PropertyDisplay { get; }
        public virtual string PropertyDescription => null;

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

        public override string TypeDefaultDescription => PropertyDescription;
        public override string TypeDefaultDisplayName => PropertyDisplay;

        public abstract VariableValue GetProperty(PositionedComponent c, List<Error> errors);

        public virtual ComponentProperty CreateVariable(PositionedComponent c)
        {
            ComponentProperty pv = (ComponentProperty)NewInstance();
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
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            if (ComponentPos != default)
            {
                Component c = BoundComponent;
                string line = $"[c/aaaa00:Bound to:] {(c is null ? "Unregistered component" : (c.TypeDisplayName ?? c.Name))} at {ComponentPos.X}, {ComponentPos.Y}";
                tooltips.Add(new(Mod, "TIPropBoundTo", line));
            }
        }

        protected override void SaveCustomData(BinaryWriter writer)
        {
            writer.Write(ComponentPos.X);
            writer.Write(ComponentPos.Y);
        }
        protected override Variable LoadCustomData(BinaryReader reader)
        {
            ComponentProperty pv = (ComponentProperty)NewInstance();

            pv.ComponentPos = new(reader.ReadInt16(), reader.ReadInt16());
            return pv;
        }

        protected override TagCompound SaveCustomTag()
        {
            return new TagCompound()
            {
                ["comx"] = ComponentPos.X,
                ["comy"] = ComponentPos.Y,
            };
        }
        protected override Variable LoadCustomTag(TagCompound data)
        {
            ComponentProperty pv = (ComponentProperty)NewInstance();

            Point16 pos = default;

            if (data.ContainsKey("comx"))
                pos.X = data.GetShort("comx");

            if (data.ContainsKey("comy"))
                pos.Y = data.GetShort("comy");

            pv.ComponentPos = pos;

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
            Register(property, true);
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

                return BoundComponentCache?.TypeName;
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
        public virtual ComponentProperty CreateVariable(TComponent component, Point16 pos) => (ComponentProperty)NewInstance();

        public sealed override VariableValue GetProperty(PositionedComponent c, List<Error> errors)
        {
            return GetProperty(c.Component as TComponent, c.Pos, errors);
        }
    }
}
