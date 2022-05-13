using System;
using System.Collections.Generic;
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

        public HashSet<PositionedComponent> AllComponents { get; } = new();
        public HashSet<PositionedComponent> ComponentsWithVariables { get; } = new();
        public Dictionary<string, HashSet<PositionedComponent>> ComponentsByType { get; } = new();
        public Dictionary<Point16, Component> ComponentsByPos { get; } = new();

        private HashSet<Guid> GetVariableSet = new();

        public Guid TempId { get; } = Guid.NewGuid();

        internal ComponentSystem() { }

        public static ComponentSystem UpdateSystem(Point16 pos)
        {
            if (Main.netMode == NetmodeID.MultiplayerClient) return null;

            Tile t = Main.tile[pos.X, pos.Y];
            if (!t.HasTile || !Component.TileTypes.Contains(t.TileType))
            {
                HashSet<Guid> ids = new();

                foreach (Point16 component in GetComponentsAround(pos))
                {
                    ComponentData data = World.GetDataOrNull(component);
                    if (data is not null && data.System is not null && ids.Contains(data.System.TempId)) continue;
                    ComponentSystem sys = UpdateSystem(component);
                    if (sys is not null) ids.Add(sys.TempId);
                }

                return null;
            }

            ComponentSystem system = new();

            foreach (PositionedComponent component in ComponentRunner(pos))
            {
                system.AllComponents.Add(component);

                if (component.Component.VariableSlots > 0)
                    system.ComponentsWithVariables.Add(component);

                string type = component.Component.ComponentType;
                if (!system.ComponentsByType.TryGetValue(type, out var componentsByType))
                {
                    componentsByType = new();
                    system.ComponentsByType[type] = componentsByType;
                }
                componentsByType.Add(component);

                system.ComponentsByPos[component.Pos] = component.Component;

                component.Component.GetData(component.Pos).System = system;
            }
            foreach (PositionedComponent component in system.AllComponents)
                component.Component.OnSystemUpdate(component.Pos);

            return system;
        }

        public static IEnumerable<PositionedComponent> ComponentRunner(Point16 pos)
        {
            HashSet<Point16> found = new();
            Queue<Point16> queue = new();
            queue.Enqueue(pos);

            while (queue.Count > 0)
            {
                Point16 p = queue.Dequeue();
                if (found.Contains(p))
                    continue;

                found.Add(p);
                Tile t = Main.tile[p.X, p.Y];
                if (Component.TileTypes.Contains(t.TileType))
                    yield return new(p, Component.ByTileType[t.TileType]);

                Point16 check = new(p.X, p.Y - 1);
                t = Main.tile[check.X, check.Y];
                if (t.HasTile && Component.TileTypes.Contains(t.TileType) && !found.Contains(check))
                    queue.Enqueue(check);

                check = new(p.X - 1, p.Y);
                t = Main.tile[check.X, check.Y];
                if (t.HasTile && Component.TileTypes.Contains(t.TileType) && !found.Contains(check))
                    queue.Enqueue(check);

                check = new(p.X, p.Y + 1);
                t = Main.tile[check.X, check.Y];
                if (t.HasTile && Component.TileTypes.Contains(t.TileType) && !found.Contains(check))
                    queue.Enqueue(check);

                check = new(p.X + 1, p.Y);
                t = Main.tile[check.X, check.Y];
                if (t.HasTile && Component.TileTypes.Contains(t.TileType) && !found.Contains(check))
                    queue.Enqueue(check);
            }
        }

        public static IEnumerable<Point16> GetComponentsAround(Point16 p)
        {
            Point16 check = new(p.X, p.Y - 1);
            Tile t = Main.tile[check.X, check.Y];
            if (t.HasTile && Component.TileTypes.Contains(t.TileType))
                yield return check;

            check = new(p.X - 1, p.Y);
            t = Main.tile[check.X, check.Y];
            if (t.HasTile && Component.TileTypes.Contains(t.TileType))
                yield return check;

            check = new(p.X, p.Y + 1);
            t = Main.tile[check.X, check.Y];
            if (t.HasTile && Component.TileTypes.Contains(t.TileType))
                yield return check;

            check = new(p.X + 1, p.Y);
            t = Main.tile[check.X, check.Y];
            if (t.HasTile && Component.TileTypes.Contains(t.TileType))
                yield return check;
        }

        public VariableValue GetVariableValue(Guid varId, List<Error> errors)
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

        public Variable GetVariable(Guid varId, List<Error> errors)
        {
            Statistics.VariableRequests++;
            Statistics.Start(Statistics.UpdateTime.VariableRequests);
            try
            {
                if (GetVariableSet.Contains(varId))
                {
                    errors.Add(new(ErrorType.RecursiveReference, World.Guids.GetShortGuid(varId)));
                    return null;
                }

                bool found = false;
                Variable result = null;

                foreach (PositionedComponent c in ComponentsWithVariables)
                {
                    ComponentData d = c.GetData();
                    foreach (Items.Variable var in d.Variables)
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
                        errors.Add(new(ErrorType.WrongComponentAtPos, pos.X, pos.Y, c.Type, type));
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
