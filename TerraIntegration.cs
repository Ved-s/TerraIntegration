using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
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
			PropertyVariable.Unregister();

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

            On.Terraria.WorldGen.KillTile += WorldGen_KillTile;
            On.Terraria.TileObject.Place += TileObject_Place;

            WorldFile.OnWorldLoad += WorldFile_OnWorldLoad;
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
			PropertyVariable.Unregister();

			VariableRenderer.Unload();

			On.Terraria.WorldGen.KillTile -= WorldGen_KillTile;
			On.Terraria.TileObject.Place -= TileObject_Place;

			WorldFile.OnWorldLoad -= WorldFile_OnWorldLoad;
		}

        private void WorldFile_OnWorldLoad()
        {
			HashSet<Point16> updated = new();

			for (int y = 0; y < Main.maxTilesY; y++)
				for (int x = 0; x < Main.maxTilesX; x++)
				{
					Tile t = Main.tile[x, y];
					if (!t.HasTile) continue;
					if (!Component.TileTypes.Contains(t.TileType)) continue;
					Component.ByTileType[t.TileType].OnLoaded(new(x, y));
					if (updated.Contains(new(x, y))) continue;

					ComponentSystem system = ComponentSystem.UpdateSystem(new(x, y));
					updated.UnionWith(system.ComponentsByPos.Keys);
				}
		}
        private void WorldGen_KillTile(On.Terraria.WorldGen.orig_KillTile orig, int i, int j, bool fail, bool effectOnly, bool noItem)
        {
			int type = Main.tile[i, j].TileType;
			orig(i, j, fail, effectOnly, noItem);
			if (!Main.tile[i, j].HasTile && Component.TileTypes.Contains(type))
			{
				Component.ByTileType[type].OnKilled(new(i, j));
			}
        }
		private bool TileObject_Place(On.Terraria.TileObject.orig_Place orig, TileObject toBePlaced)
		{
			bool result = orig(toBePlaced);
			if (Component.ByTileType.TryGetValue(toBePlaced.type, out Component c))
				c.OnPlaced(new(toBePlaced.xCoord, toBePlaced.yCoord));

			return result;
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

			PropertyVariable.ComponentRegistered();
		}
		public void RegisterVariable(Variable v)
		{
			if (v is PropertyVariable pv)
			{
				PropertyVariable.Register(pv);
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