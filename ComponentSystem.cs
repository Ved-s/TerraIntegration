using System;
using System.Collections.Generic;
using System.Linq;
using TerraIntegration.Components;
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
        public static ComponentWorld World => ModContent.GetInstance<ComponentWorld>();

        public static HashSet<int> CableTiles = new();
        public static HashSet<int> CableWalls = new();

        public HashSet<WorldPoint> AllPoints { get; } = new();
        public HashSet<PositionedComponent> AllComponents { get; } = new();
        public HashSet<PositionedComponent> ComponentsWithVariables { get; } = new();
        public Dictionary<string, HashSet<PositionedComponent>> ComponentsByType { get; } = new();
        public Dictionary<Point16, Component> ComponentsByPos { get; } = new();

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
            HashSet<WorldPoint> checks = new();
            systems = new();

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

                foreach (PositionedComponent component in system.AllComponents)
                    component.Component.OnSystemUpdate(component.Pos);

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
                PositionedComponent positioned = new(point.ToPoint16(), component);

                AllComponents.Add(positioned);

                if (component.CanHaveVariables)
                    ComponentsWithVariables.Add(positioned);

                string type = component.ComponentType;
                if (!ComponentsByType.TryGetValue(type, out var componentsByType))
                {
                    componentsByType = new();
                    ComponentsByType[type] = componentsByType;
                }
                componentsByType.Add(positioned);

                ComponentsByPos[positioned.Pos] = component;

                positioned.GetData().System = this;
            }
        }

        public VariableValue GetVariableValue(Guid varId, List<Error> errors)
        {
            if (GetVariableSet.Contains(varId))
            {
                errors.Add(new(ErrorType.RecursiveReference, World.Guids.GetShortGuid(varId)));
                return null;
            }
            GetVariableSet.Add(varId);
            try
            {
                Variable var = GetVariable(varId, errors);
                if (var is null) return null;

                VariableValue val = var.GetValue(this, errors);
                if (val is null) return null;

                if (val is UnloadedVariableValue)
                {
                    errors.Add(new(ErrorType.ValueUnloaded, World.Guids.GetShortGuid(varId)));
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
            Statistics.VariableRequests++;
            Statistics.Start(Statistics.UpdateTime.VariableRequests);

            try
            {
                bool found = false;
                Variable result = null;

                foreach (PositionedComponent c in ComponentsWithVariables)
                {
                    ComponentData d = c.GetData();
                    foreach (Items.Variable var in d.Variables.Values)
                        if (var is not null)
                            if (var.Var.Id == varId)
                            {
                                if (var.Var is UnloadedVariable)
                                {
                                    errors.Add(new(ErrorType.VariableUnloaded, World.Guids.GetShortGuid(varId)));
                                    return null;
                                }
                                if (found)
                                {
                                    errors.Add(new(ErrorType.MultipleVariablesSameID, World.Guids.GetShortGuid(varId)));
                                    return null;
                                }
                                found = true;
                                result = var.Var;
                            }
                }
                if (result is not null)
                    return result;

                errors.Add(new(ErrorType.VariableNotFound, World.Guids.GetShortGuid(varId)));
                return null;
            }
            finally
            {
                Statistics.Stop(Statistics.UpdateTime.VariableRequests);
            }
        }
        public Component GetComponent(Point16 pos, string type, List<Error> errors)
        {
            Statistics.ComponentRequests++;
            Statistics.Start(Statistics.UpdateTime.ComponentRequests);
            try
            {
                if (ComponentsByPos.TryGetValue(pos, out Component c))
                {
                    if (type is not null && c.ComponentType != type)
                    {
                        errors.Add(new(ErrorType.WrongComponentAtPos, pos.X, pos.Y, c.ComponentType, type));
                        return null;
                    }
                    return c;
                }

                errors.Add(new(ErrorType.NoComponentAtPos, pos.X, pos.Y, type));

                return null;
            }
            finally
            {
                Statistics.Stop(Statistics.UpdateTime.ComponentRequests);
            }
        }
    }
}
