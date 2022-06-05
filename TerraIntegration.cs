using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using TerraIntegration.Basic;
using TerraIntegration.Components;
using TerraIntegration.DataStructures;
using TerraIntegration.DisplayedValues;
using TerraIntegration.Values;
using TerraIntegration.Variables;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace TerraIntegration
{
    public class TerraIntegration : Mod
    {
        public static bool DebugMode => Debugger.IsAttached;

        public override void Load()
        {
            Unregister();

            ComponentSystem.RegisterCable(ModContent.TileType<Tiles.Cable>(), false);
            ComponentSystem.RegisterCable(ModContent.WallType<Walls.Cable>(), true);

            Assembly asm = Assembly.GetExecutingAssembly();
            foreach (Type t in asm.GetTypes())
            {
                if (t.IsAbstract) continue;

                if (t.GetCustomAttribute<AutoloadAttribute>()?.Value == false)
                    continue;

                if (t.IsSubclassOf(typeof(Component)))
                {
                    Type generic = typeof(ContentInstance<>).MakeGenericType(t);
                    Component c = (Component)generic.GetProperty("Instance").GetValue(null);

                    Component.Register(c);
                }
                else if (t.IsSubclassOf(typeof(Variable)))
                {
                    Variable.Register(Activator.CreateInstance(t) as Variable);
                }
                else if (t.IsSubclassOf(typeof(VariableValue)))
                {
                    VariableValue.Register(Activator.CreateInstance(t) as VariableValue);
                }
                else if (t.IsSubclassOf(typeof(DisplayedValue)))
                {
                    DisplayedValue.Register(Activator.CreateInstance(t) as DisplayedValue);
                }
            }

            VariableRenderer.TypeSpritesheetOverrides[typeof(Interfaces.ICollection)] = new(VariableValue.BasicSheet, 0, 2);
            VariableRenderer.TypeSpritesheetOverrides[typeof(Interfaces.ICollection<>)] = new(VariableValue.BasicSheet, 0, 2);
        }
        public override void Unload()
        {
            Unregister();
            VariableRenderer.Unload();
        }

        private static void Unregister()
        {
            Component.Unregister();
            Variable.Unregister();
            VariableValue.Unregister();

            ComponentSystem.Unregister();
        }

        public override void HandlePacket(BinaryReader reader, int whoAmI)
        {
            Networking.HandlePacket(reader, whoAmI);
        }

        public override void AddRecipeGroups()
        {
            RecipeGroup.RegisterGroup("T2Watch", new(() => "Any tier 2 watches", ItemID.SilverWatch, ItemID.TungstenWatch));
        }
    }

    public record struct PositionedComponent(Point16 Pos, Component Component)
    {
        public ComponentData GetData() => ModContent.GetInstance<ComponentWorld>().GetData(Pos, Component);
        public ComponentData GetDataOrNull() => ModContent.GetInstance<ComponentWorld>().GetDataOrNull(Pos);

        public T GetData<T>() where T : ComponentData, new()
            => ModContent.GetInstance<ComponentWorld>().GetData<T>(Pos, Component);
        public T GetDataOrNull<T>() where T : ComponentData, new()
            => ModContent.GetInstance<ComponentWorld>().GetDataOrNull<T>(Pos);
    }

    public enum CallSide { Both, Client, Server }
    public class CallSideAttribute : Attribute
    {
        public CallSide Side { get; set; }
        public CallSideAttribute(CallSide side) { Side = side; }
    }
}