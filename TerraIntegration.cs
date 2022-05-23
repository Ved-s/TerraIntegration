using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using TerraIntegration.ComponentProperties;
using TerraIntegration.Components;
using TerraIntegration.Values;
using TerraIntegration.Variables;
using Terraria;
using Terraria.DataStructures;
using Terraria.IO;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace TerraIntegration
{
	public class TerraIntegration : Mod
	{
		public static bool DebugMode => Debugger.IsAttached;

		public override void Load()
        {
            Unregister();

            Variable.Register(new Variable());
            VariableValue.Register(new VariableValue());

            Assembly asm = Assembly.GetExecutingAssembly();
            foreach (Type t in asm.GetTypes())
            {
                if (t.IsAbstract) continue;

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
            }
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