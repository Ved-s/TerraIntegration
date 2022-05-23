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

        public static HashSet<int> CableTiles = new() { ModContent.TileType<Tiles.Cable>() };
        public static HashSet<int> CableWalls = new() { ModContent.WallType<Walls.CableWall>() };

        public HashSet<WorldPoint> AllPoints { get; } = new();
        public HashSet<PositionedComponent> AllComponents { get; } = new();
        public HashSet<PositionedComponent> ComponentsWithVariables { get; } = new();
        public Dictionary<string, HashSet<PositionedComponent>> ComponentsByType { get; } = new();
        public Dictionary<Point16, Component> ComponentsByPos { get; } = new();

        private HashSet<Guid> GetVariableSet = new();

        public Guid TempId { get; } = Guid.NewGuid();

        internal ComponentSystem() { }

        //public static void UpdateSystem(IEnumerable<Point16> positions)
        //{
        //    HashSet<Guid> ids = new();

        //    foreach (Point16 pos in positions)
        //    {
        //        Tile t = Framing.GetTileSafely(pos);
        //        if (!t.HasTile || !Component.TileTypes.Contains(t.TileType)) continue;

        //        ComponentData data = World.GetData(pos);
        //        if (data is not null && data.System is not null && ids.Contains(data.System.TempId)) continue;
        //        ComponentSystem sys = UpdateSystem(pos);
        //        if (sys is not null) ids.Add(sys.TempId);
        //    }
        //}

        //public static ComponentSystem UpdateSystem(Point16 pos)
        //{
        //    if (Main.netMode == NetmodeID.MultiplayerClient) return null;

        //    Tile t = Main.tile[pos.X, pos.Y];
        //    if (!t.HasTile || !Component.TileTypes.Contains(t.TileType))
        //    {
        //        HashSet<Guid> ids = new();

        //        foreach (Point16 component in GetComponentsAround(pos))
        //        {
        //            ComponentData data = World.GetData(component);
        //            if (data is not null && data.System is not null && ids.Contains(data.System.TempId)) continue;
        //            ComponentSystem sys = UpdateSystem(component);
        //            if (sys is not null) ids.Add(sys.TempId);
        //        }

        //        return null;
        //    }

        //    ComponentSystem system = new();

        //    foreach (PositionedComponent component in ComponentRunner(pos))
        //    {
        //        system.AllComponents.Add(component);

        //        if (component.Component.CanHaveVariables)
        //            system.ComponentsWithVariables.Add(component);

        //        string type = component.Component.ComponentType;
        //        if (!system.ComponentsByType.TryGetValue(type, out var componentsByType))
        //        {
        //            componentsByType = new();
        //            system.ComponentsByType[type] = componentsByType;
        //        }
        //        componentsByType.Add(component);

        //        system.ComponentsByPos[component.Pos] = component.Component;

        //        component.Component.GetData(component.Pos).System = system;
        //    }
        //    foreach (PositionedComponent component in system.AllComponents)
        //        component.Component.OnSystemUpdate(component.Pos);

        //    return system;
        //}

        //public static void UpdateSystemWalls(Point16 pos) => UpdateSystem(CableWallRunner(pos));

        //public static IEnumerable<PositionedComponent> ComponentRunner(Point16 pos)
        //{
        //    HashSet<Point16> found = new();
        //    HashSet<Point16> foundWalls = new();
        //    Queue<Point16> queue = new();
        //    queue.Enqueue(pos);

        //    int cableWallType = ModContent.WallType<Walls.CableWall>();

        //    while (queue.Count > 0)
        //    {
        //        Point16 p = queue.Dequeue();
        //        if (found.Contains(p))
        //            continue;

        //        found.Add(p);
        //        Tile t = Main.tile[p.X, p.Y];

        //        if (t.WallType == cableWallType && !foundWalls.Contains(p))
        //        {
        //            foreach (Point16 wallpos in CableWallRunner(p))
        //                if (!foundWalls.Contains(wallpos))
        //                {
        //                    foundWalls.Add(wallpos);
        //                    if (!found.Contains(wallpos))
        //                        queue.Enqueue(wallpos);
        //                }
        //        }
        //        if (Component.TileTypes.Contains(t.TileType))
        //            yield return new(p, Component.ByTileType[t.TileType]);
        //        else continue;

        //        Point16 check = new(p.X, p.Y - 1);
        //        t = Main.tile[check.X, check.Y];
        //        if (t.HasTile && !found.Contains(check))
        //            queue.Enqueue(check);

        //        check = new(p.X - 1, p.Y);
        //        t = Main.tile[check.X, check.Y];
        //        if (t.HasTile && !found.Contains(check))
        //            queue.Enqueue(check);

        //        check = new(p.X, p.Y + 1);
        //        t = Main.tile[check.X, check.Y];
        //        if (t.HasTile && !found.Contains(check))
        //            queue.Enqueue(check);

        //        check = new(p.X + 1, p.Y);
        //        t = Main.tile[check.X, check.Y];
        //        if (t.HasTile && !found.Contains(check))
        //            queue.Enqueue(check);
        //    }
        //}

        //public static IEnumerable<Point16> CableWallRunner(Point16 pos)
        //{
        //    HashSet<Point16> found = new();
        //    Queue<Point16> queue = new();
        //    queue.Enqueue(new(pos.X, pos.Y - 1));
        //    queue.Enqueue(new(pos.X, pos.Y + 1));
        //    queue.Enqueue(new(pos.X - 1, pos.Y));
        //    queue.Enqueue(new(pos.X + 1, pos.Y));
        //    queue.Enqueue(pos);

        //    int cableWallType = ModContent.WallType<Walls.CableWall>();

        //    while (queue.Count > 0)
        //    {
        //        Point16 p = queue.Dequeue();
        //        if (found.Contains(p))
        //            continue;

        //        found.Add(p);
        //        Tile t = Main.tile[p.X, p.Y];

        //        if (t.WallType != cableWallType) continue;

        //        yield return p;

        //        Point16 check = new(p.X, p.Y - 1);
        //        t = Main.tile[check.X, check.Y];
        //        if (!found.Contains(check))
        //            queue.Enqueue(check);

        //        check = new(p.X - 1, p.Y);
        //        t = Main.tile[check.X, check.Y];
        //        if (!found.Contains(check))
        //            queue.Enqueue(check);

        //        check = new(p.X, p.Y + 1);
        //        t = Main.tile[check.X, check.Y];
        //        if (!found.Contains(check))
        //            queue.Enqueue(check);

        //        check = new(p.X + 1, p.Y);
        //        t = Main.tile[check.X, check.Y];
        //        if (!found.Contains(check))
        //            queue.Enqueue(check);
        //    }
        //}

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

        public static IEnumerable<WorldPoint> GetConnectorsAround(WorldPoint p)
        {
            if (p.Wall && IsConnector(p.WithWall(false)))
                yield return p.WithWall(false);

            if (!p.Wall && IsConnector(p.WithWall(true)))
                yield return p.WithWall(true);

            WorldPoint check = p.WithOffset(0, -1);
            if (IsConnector(check)) yield return check;

            check = p.WithOffset(0, 1);
            if (IsConnector(check)) yield return check;

            check = p.WithOffset(-1, 0);
            if (IsConnector(check)) yield return check;

            check = p.WithOffset(1, 0);
            if (IsConnector(check)) yield return check;
        }
        public static bool IsConnector(WorldPoint p)
        {
            Tile t = Framing.GetTileSafely(p.ToPoint());

            if (p.Wall)
                return CableWalls.Contains(t.WallType);
            else
                return CableTiles.Contains(t.TileType) || Component.TileTypes.Contains(t.TileType);
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

                Tile t = Framing.GetTileSafely(p.ToPoint());

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

        public void Add(WorldPoint point)
        {
            AllPoints.Add(point);
            Tile t = Framing.GetTileSafely(point.ToPoint());
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
