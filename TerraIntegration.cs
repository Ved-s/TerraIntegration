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
            Component.TileTypes.Clear();
			Component.ByType.Clear();
			Component.ByTypeName.Clear();
			Component.ByTileType.Clear();
			Variable.ByTypeName.Clear();
			VariableValue.ByTypeName.Clear();
			VariableValue.ByType.Clear();
			ComponentProperty.Unregister();
			ValueProperty.Unregister();

			RegisterVariable(new Variable());
			RegisterVariableValue(new VariableValue());

			Assembly asm = Assembly.GetExecutingAssembly();
			foreach (Type t in asm.GetTypes())
			{
				if (t.IsAbstract) continue;

				if (t.IsSubclassOf(typeof(Component)))
				{
					Type generic = typeof(ContentInstance<>).MakeGenericType(t);
					Component c = (Component)generic.GetProperty("Instance").GetValue(null);

					RegisterComponent(c);
				}
				else if (t.IsSubclassOf(typeof(Variable)))
				{
					RegisterVariable(Activator.CreateInstance(t) as Variable);
				}
				else if (t.IsSubclassOf(typeof(VariableValue)))
				{
					RegisterVariableValue(Activator.CreateInstance(t) as VariableValue);
				}
			}
        }

        public override void Unload()
        {
			Component.TileTypes.Clear();
			Component.ByType.Clear();
			Component.ByTypeName.Clear();
			Component.ByTileType.Clear();
			Variable.ByTypeName.Clear();
			VariableValue.ByTypeName.Clear();
			VariableValue.ByType.Clear();
			ComponentProperty.Unregister();
			ValueProperty.Unregister();

			VariableRenderer.Unload();
		}

		public override void HandlePacket(BinaryReader reader, int whoAmI)
        {
            Networking.HandlePacket(reader, whoAmI);
        }

        public void RegisterComponent(Component c)
		{
			if (c?.ComponentType is null) return;

			Component.TileTypes.Add(c.Type);
			Component.ByType[c.GetType()] = c;
			Component.ByTileType[c.Type] = c;
			Component.ByTypeName[c.ComponentType] = c;

			ComponentProperty.ComponentRegistered();
		}
		public void RegisterVariable(Variable v)
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
			Variable.ByTypeName[v.Type] = v;
		}
		public void RegisterVariableValue(VariableValue v)
		{
			if (v?.Type is null) return;

			VariableValue.ByTypeName[v.Type] = v;
			VariableValue.ByType[v.GetType()] = v;

			ValueProperty.ValueRegistered();
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