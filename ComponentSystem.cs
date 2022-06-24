using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using TerraIntegration.Basic;
using TerraIntegration.Components;
using TerraIntegration.DataStructures;
using TerraIntegration.Stats;
using TerraIntegration.Values;
using TerraIntegration.Variables;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace TerraIntegration
{
    public class ComponentSystem
    {
        public static TerraIntegration Mod => ModContent.GetInstance<TerraIntegration>();
        public static ComponentWorld World => ComponentWorld.Instance;

        public static HashSet<int> CableTiles = new();
        public static HashSet<int> CableWalls = new();

        public HashSet<WorldPoint> AllPoints { get; } = new();
        HashSet<Point16> datas = new();
        public Dictionary<Point16, Component> AllComponents { get; } = new();
        public Dictionary<Point16, Component> ComponentsWithVariables { get; } = new();
        public Dictionary<string, Dictionary<Point16, Component>> ComponentsByType { get; } = new();

        public Dictionary<Guid, (Point16 pos, string slot)> VariableCache { get; } = new();

        private HashSet<Guid> GetVariableSet = new();

        public Guid TempId { get; } = Guid.NewGuid();

        internal ComponentSystem() { }

        public static void RegisterCable(int type, bool wall)
        {
            if (wall) CableWalls.Add(type);
            else CableTiles.Add(type);
        }
        public static void UpdateSystem(WorldPoint start, out List<ComponentSystem> systems)
        {
            systems = new();
            if (Networking.Client) return;

            HashSet<WorldPoint> checks = new();

            if (IsConnector(start)) checks.Add(start);
            else checks.UnionWith(GetConnectorsAround(start));

            while (checks.Count > 0)
            {
                WorldPoint check = checks.First();
                checks.Remove(check);

                ComponentSystem system = new();

                foreach (WorldPoint p in EnumerateConnectors(check))
                {
                    checks.Remove(p);
                    system.Add(p);
                }

                foreach (var component in system.AllComponents)
                    component.Value?.OnSystemUpdate(component.Key);

                systems.Add(system);
            }
        }

        internal static void Unregister() 
        {
            CableTiles.Clear();
            CableWalls.Clear();
        }

        public static IEnumerable<WorldPoint> GetConnectorsAround(WorldPoint p)
        {
            WorldPoint check = p.WithWall(false);
            if (p.Wall && IsConnector(check))
                yield return check;

            check = p.WithWall(true);
            if (!p.Wall && IsConnector(check))
                yield return check;

            check = p.WithOffset(0, -1);
            if (IsConnector(check))
                yield return check;

            check = p.WithOffset(0, 1);
            if (IsConnector(check))
                yield return check;

            check = p.WithOffset(-1, 0);
            if (IsConnector(check)) 
                yield return check;

            check = p.WithOffset(1, 0);
            if (IsConnector(check)) 
                yield return check;
        }
        public static IEnumerable<WorldPoint> EnumerateConnectors(WorldPoint start)
        {
            HashSet<WorldPoint> found = new();
            Queue<WorldPoint> queue = new();
            queue.Enqueue(start);

            while (queue.Count > 0)
            {
                WorldPoint p = queue.Dequeue();
                if (found.Contains(p)) continue;
                found.Add(p);

                Terraria.Tile t = Framing.GetTileSafely(p.ToPoint());

                if (!IsConnector(p)) continue;

                if (p.Wall && IsConnector(p.WithWall(false)))
                    queue.Enqueue(p.WithWall(false));

                if (!p.Wall && IsConnector(p.WithWall(true)))
                    queue.Enqueue(p.WithWall(true));

                yield return p;

                queue.Enqueue(p.WithOffset(0, -1));
                queue.Enqueue(p.WithOffset(1, 0));
                queue.Enqueue(p.WithOffset(0, 1));
                queue.Enqueue(p.WithOffset(-1, 0));
            }
        }
        public static bool IsConnector(WorldPoint p)
        {
            Terraria.Tile t = Framing.GetTileSafely(p.ToPoint());

            if (p.Wall)
                return CableWalls.Contains(t.WallType);
            else
                return CableTiles.Contains(t.TileType) || Component.TileTypes.Contains(t.TileType);
        }

        public void Add(WorldPoint point)
        {
            AllPoints.Add(point);
            Terraria.Tile t = Framing.GetTileSafely(point.ToPoint());
            if (!point.Wall && Component.ByTileType.TryGetValue(t.TileType, out Component component))
            {
                Point16 pos = point.ToPoint16();

                ComponentData data = World.GetData(pos, null, false);
                if (data is SubTileComponentData)
                    return;

                AllComponents.Add(pos, component);

                if (component.CanHaveVariables)
                    ComponentsWithVariables.Add(pos, component);

                string type = component.TypeName;
                if (!ComponentsByType.TryGetValue(type, out var componentsByType))
                {
                    componentsByType = new();
                    ComponentsByType[type] = componentsByType;
                }
                componentsByType.Add(pos, component);

                data.System = this;
            }
        }

        public VariableValue GetVariableValue(Guid varId, List<Error> errors)
        {
            if (GetVariableSet.Contains(varId))
            {
                errors.Add(Errors.RecursiveReference(varId));
                return null;
            }
            GetVariableSet.Add(varId);
            try
            {
                Variable var = GetVariable(varId, errors);
                if (var is null) return null;

                VariableValue val = var.GetValue(this, errors);
                var.SetLastValue(val, this);
                if (val is null) return null;

                if (val is UnloadedVariableValue)
                {
                    errors.Add(Errors.ValueUnloaded(varId));
                    return null;
                }

                return val;
            }
            finally
            {
                GetVariableSet.Remove(varId);
            }
        }
        public Variable GetVariable(Guid varId, List<Error> errors)
        {
            Statistics.VariablesRequested.Increase();
            Statistics.VariableRequests.Start();

            bool needsCleaning = false;

            try
            {
                if (VariableCache.TryGetValue(varId, out var varCache)
                    && ComponentsWithVariables.ContainsKey(varCache.pos))
                {
                    ComponentData data = World.GetDataOrNull(varCache.pos);
                    if (data is not null)
                    {
                        Variable var = data.GetVariable(varCache.slot);
                        if (var is not null)
                            return var;
                    }
                }

                bool found = false;
                Variable result = null;
                Point16 foundPos = default;
                string foundSlot = null;

                foreach (var com in ComponentsWithVariables)
                {
                    ComponentData d = com.Value?.GetDataOrNull(com.Key, false);
                    if (d is null || d is SubTileComponentData)
                    {
                        needsCleaning = true;
                        continue;
                    }

                    foreach (var var in d.Variables)
                        if (var.Value is not null)
                            if (var.Value.Var?.Id == varId)
                            {
                                if (var.Value.Var is UnloadedVariable)
                                {
                                    errors.Add(Errors.VariableUnloaded(varId));
                                    return null;
                                }
                                if (found)
                                {
                                    errors.Add(Errors.MultipleVariablesSameID(varId));
                                    return null;
                                }
                                found = true;
                                foundPos = com.Key;
                                foundSlot = var.Key;
                                result = var.Value.Var;
 
                            }
                }
                if (result is not null)
                {
                    if (foundPos != default && foundSlot is not null)
                        VariableCache[varId] = (foundPos, foundSlot);

                    return result;
                }

                errors.Add(Errors.VariableNotFound(varId));
                return null;
            }
            finally
            {
                if (needsCleaning)
                    CleanSystem();

                Statistics.VariableRequests.Stop();
            }
        }
        public Component GetComponent(Point16 pos, string type, List<Error> errors)
        {
            Statistics.ComponentsRequested.Increase();
            Statistics.ComponentRequests.Start();
            try
            {
                if (AllComponents.TryGetValue(pos, out Component c))
                {
                    if (type is not null && c.TypeName != type)
                    {
                        errors.Add(Errors.WrongComponentAtPos(pos, c.TypeName, type));
                        return null;
                    }
                    return c;
                }

                errors.Add(Errors.NoComponentAtPos(pos, type));

                return null;
            }
            finally
            {
                Statistics.ComponentRequests.Stop();
            }
        }

        public TVariable GetVariable<TVariable>(Guid varId, List<Error> errors, TypeIdentity id) where TVariable : Variable
        {
            Variable var = GetVariable(varId, errors);
            if (var is null) return null;

            if (var is not TVariable tv)
            {
                errors.Add(Errors.ExpectedVariable(typeof(TVariable), id));
                return null;
            }
            return tv;
        }

        void CleanSystem() 
        {
            List<Point16> removePoints = new();

            foreach (Point16 p in AllComponents.Keys)
                if (World.GetDataOrNull(p) is null or SubTileComponentData)
                    removePoints.Add(p);

            foreach (Point16 p in removePoints)
            {
                AllComponents.Remove(p);
                ComponentsWithVariables.Remove(p);

                foreach (var value in ComponentsByType.Values)
                    value.Remove(p);
            }
        }
    }
}
