using CustomTreeLib;
using CustomTreeLib.DataStructures;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using TerraIntegration.Basic;
using TerraIntegration.DataStructures;
using TerraIntegration.Stats;
using TerraIntegration.UI;
using TerraIntegration.Utilities;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.UI;

namespace TerraIntegration
{
    public class ComponentWorld : ModSystem
    {
        public new static TerraIntegration Mod => ModContent.GetInstance<TerraIntegration>();
        public static ComponentWorld Instance => ModContent.GetInstance<ComponentWorld>();

        public ShortGuids Guids { get; } = new(2);

        private GrowingList<ComponentData> ComponentData = new();
        public Dictionary<Point16, Component> ComponentUpdates = new();

        public uint UpdateCounter = 0;

        public Item HoverItem = null;
        private string HoverText = null;
        public bool ComponentDebug = false;

        public readonly List<DelayedWireTrip> WireTrips = new();

        private Texture2D HighlightTexture;

        public readonly List<VariableMatchDelegate> VariableHighlights = new();

        private UnloadedComponent UnloadedComponent = new();

        public T GetData<T>(Point16 pos, Component c = null, bool resolveSubTiles = true) where T : ComponentData, new()
        {
            Statistics.ComponentDatasRequested.Increase();
            Statistics.ComponentDataRequests.Start();
            try
            {
                if (TryFindData(pos, out ComponentData data) && data is not null)
                {
                    if (data is SubTileComponentData subTile)
                    {
                        if (!subTile.CheckValid())
                            RemoveAll(pos);
                        else if (resolveSubTiles)
                            return GetData<T>(subTile.MainTilePos, c, resolveSubTiles);
                    }

                    if (data is T tdata)
                        return tdata;
                }

                RemoveAll(pos);

                if (c is null)
                {
                    Tile t = Main.tile[pos.X, pos.Y];
                    if (!Component.ByTileType.TryGetValue(t.TileType, out c))
                        return null;
                }

                return InitData<T>(pos, c, data);
            }
            finally
            {
                Statistics.ComponentDataRequests.Stop();
            }
        }
        public ComponentData GetData(Point16 pos, Component c = null, bool resolveSubTiles = true)
        {
            Statistics.ComponentDatasRequested.Increase();
            Statistics.ComponentDataRequests.Start();
            try
            {
                if (TryFindData(pos, out ComponentData data) && data is not null)
                {
                    if (data is SubTileComponentData subTile)
                    {
                        if (!subTile.CheckValid())
                            RemoveAll(pos);
                        else if (resolveSubTiles)
                            return GetData(subTile.MainTilePos, c, resolveSubTiles);
                    }

                    return data;
                }

                if (c is null)
                {
                    Tile t = Main.tile[pos.X, pos.Y];
                    if (!Component.ByTileType.TryGetValue(t.TileType, out c))
                        return null;
                }

                return InitData(pos, c);
            }
            finally
            {
                Statistics.ComponentDataRequests.Stop();
            }
        }

        public T GetDataOrNull<T>(Point16 pos, bool resolveSubTiles = true) where T : ComponentData
        {
            Statistics.ComponentDatasRequested.Increase();
            Statistics.ComponentDataRequests.Start();
            try
            {
                if (TryFindData(pos, out ComponentData data) && data is T t)
                {
                    if (data is SubTileComponentData subTile)
                    {
                        if (!subTile.CheckValid())
                            RemoveAll(pos);
                        else if (resolveSubTiles)
                            return GetDataOrNull<T>(subTile.MainTilePos, resolveSubTiles);
                    }

                    return t;
                }
                return null;
            }
            finally
            {
                Statistics.ComponentDataRequests.Stop();
            }
        }
        public ComponentData GetDataOrNull(Point16 pos, bool resolveSubTiles = true)
        {
            Statistics.ComponentDatasRequested.Increase();
            Statistics.ComponentDataRequests.Start();
            try
            {
                if (TryFindData(pos, out ComponentData data))
                {
                    if (data is SubTileComponentData subTile)
                    {
                        if (!subTile.CheckValid())
                            RemoveAll(pos);
                        else if (resolveSubTiles)
                            return GetDataOrNull(subTile.MainTilePos, resolveSubTiles);
                    }

                    return data;
                }

                return null;
            }
            finally
            {
                Statistics.ComponentDataRequests.Stop();
            }
        }

        public ComponentData InitData(Point16 pos, Component c)
        {
            ComponentData newData = new();
            newData.Init(c, pos);

            int index = FindFreeDataIndex();
            ComponentData[index] = newData;

            Framing.GetTileSafely(pos).Get<ComponentDataIndex>().Index = index;

            return newData;
        }
        public T InitData<T>(Point16 pos, Component c, ComponentData oldData = null) where T : ComponentData, new()
        {
            T newData = new();
            newData.Init(c, pos);
            if (oldData is not null)
                oldData.CopyTo(newData);

            int index = FindFreeDataIndex();
            ComponentData[index] = newData;

            Framing.GetTileSafely(pos).Get<ComponentDataIndex>().Index = index;

            return newData;
        }

        public bool TryFindData(Point16 pos, out ComponentData data)
        {
            ref ComponentDataIndex index = ref Framing.GetTileSafely(pos).Get<ComponentDataIndex>();

            if (index.Index >= 0 && ComponentData[index.Index]?.Position == pos)
            {
                data = ComponentData[index.Index];
                return true;
            }

            for (int i = 0; i < ComponentData.Count; i++)
            {
                ComponentData d = ComponentData[i];
                if (d is not null && d.Position == pos)
                {
                    index.Index = i;
                    data = d;
                    return true;
                }
            }
            data = null;
            return false;
        }
        public bool TryFindDataIndex(Point16 pos, out int index)
        {
            ref ComponentDataIndex ind = ref Framing.GetTileSafely(pos).Get<ComponentDataIndex>();

            if (ind.Index >= 0 && ComponentData[ind.Index]?.Position == pos)
            {
                index = ind.Index;
                return true;
            }

            for (int i = 0; i < ComponentData.Count; i++)
            {
                ComponentData d = ComponentData[i];
                if (d is not null && d.Position == pos)
                {
                    ind.Index = i;
                    index = i;
                    return true;
                }
            }
            index = -1;
            return false;
        }
        public int FindFreeDataIndex()
        {
            for (int i = 0; i < ComponentData.Count; i++)
                if (ComponentData[i] is null)
                    return i;
            return ComponentData.Count;
        }

        public void DefineMultitile(Rectangle tileRect)
        {
            ComponentData data = GetDataOrNull(new(tileRect.X, tileRect.Y));
            if (data is not null) data.Size = new(tileRect.Width, tileRect.Height);

            for (int i = tileRect.Left; i < tileRect.Right; i++)
                for (int j = tileRect.Top; j < tileRect.Bottom; j++)
                {
                    if (i == tileRect.Left && j == tileRect.Top)
                        continue;

                    Point16 pos = new(i, j);

                    RemoveAll(pos);

                    SubTileComponentData sub = new();
                    sub.Position = pos;
                    sub.MainTilePos = new(tileRect.X, tileRect.Y);
                    SetData(pos, sub);
                }
        }

        public void SetData(Point16 pos, ComponentData data)
        {
            if (TryFindDataIndex(pos, out int index))
            {
                ComponentData[index].Destroy();
                ComponentData[index] = null;
            }

            index = FindFreeDataIndex();
            ComponentData[index] = data;
            Framing.GetTileSafely(pos).Get<ComponentDataIndex>().Index = index;
        }

        public void RemoveAll(Point16 pos)
        {
            TileMimicking.MimicData.Remove(pos);
            if (TryFindDataIndex(pos, out int dataIndex))
            {
                ComponentData[dataIndex].Destroy();
                ComponentData[dataIndex] = null;
            }
            ComponentUpdates.Remove(pos);

            Framing.GetTileSafely(pos).Get<ComponentDataIndex>().Index = -1;
        }

        public bool HasData(Point16 pos)
        {
            if (TryFindData(pos, out ComponentData data))
            {
                if (data is SubTileComponentData subTile && !subTile.CheckValid())
                {
                    RemoveAll(pos);
                    return false;
                }
                return true;
            }
            return false;
        }

        public IEnumerable<ComponentData> EnumerateAllComponentData()
        {
            foreach (var data in ComponentData)
            {
                if (data is null or SubTileComponentData)
                    continue;

                yield return data;
            }
        }

        public override void Load()
        {
            if (!Main.dedServ)
                HighlightTexture = ModContent.Request<Texture2D>($"{Mod.Name}/Assets/UI/SlotHighlight", AssetRequestMode.ImmediateLoad).Value;
        }
        public override void OnWorldUnload()
        {
            TileMimicking.Clear();
            ComponentData.Clear();
            Guids.Clear();
        }
        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
        {
            int ind = layers.FindIndex(l => l.Name == "Vanilla: Inventory");
            if (ind >= 0) layers.Insert(ind, new LegacyGameInterfaceLayer($"{Mod.Name}: UI", DrawUI, InterfaceScaleType.UI));
        }

        public override void PostUpdateEverything()
        {
            Statistics.FullUpdate.Start();

            Stopwatch watch = new();

            foreach (KeyValuePair<Point16, Component> kvp in ComponentUpdates)
            {
                ComponentData data = GetData(kvp.Key, kvp.Value);
                if (data.UpdateFrequency == 0) continue;
                if (UpdateCounter % data.UpdateFrequency == 0)
                {
                    Statistics.GetComponent(kvp.Value.TypeName).Start();

                    //DateTime updateStart = DateTime.Now;
                    watch.Restart();
                    kvp.Value.OnUpdate(kvp.Key);
                    watch.Stop();
                    data.LastUpdateTime = watch.Elapsed;
                    Statistics.ComponentsUpdated.Increase();

                    Statistics.GetComponent(kvp.Value.TypeName).Stop();
                }

            }

            if (Main.netMode != NetmodeID.Server && !Main.LocalPlayer.mouseInterface)
            {
                Point16 mouseWorldTile = (Point16)(Main.MouseWorld / 16);

                if (TryFindData(mouseWorldTile, out ComponentData cdata) && cdata.Component is not null)
                {
                    string hover = cdata.Component.GetHoverText(mouseWorldTile);
                    if (hover is not null)
                        AddHoverText(hover);
                }
            }

            List<DelayedWireTrip> tripped = new();
            foreach (DelayedWireTrip trip in WireTrips)
            {
                if (trip.Delay <= 0)
                {
                    Wiring.TripWire(trip.X, trip.Y, trip.Width, trip.Height);
                    tripped.Add(trip);
                }
                else trip.Delay--;
            }
            foreach (DelayedWireTrip trip in tripped)
                WireTrips.Remove(trip);

            UpdateCounter++;
            Statistics.FullUpdate.Stop();
        }
        public override void PostDrawTiles()
        {
            if (ComponentDebug)
            {
                Main.spriteBatch.Begin();
                Rectangle screen = new(0, 0, Main.screenWidth, Main.screenHeight);
                foreach (ComponentData data in ComponentData)
                {
                    Point tl = Util.WorldToScreen(data.Position).ToPoint();
                    Point br = Util.WorldToScreen(data.Position + data.Size).ToPoint();
                    Rectangle rect = new(tl.X, tl.Y, br.X - tl.X, br.Y - tl.Y);
                    if (!rect.Intersects(screen))
                        continue;

                    if (data is SubTileComponentData)
                    {
                        Drawing.DrawRect(Main.spriteBatch, rect, null, Color.Yellow * .2f);
                        continue;
                    }
                    uint color = ((uint?)data.System?.TempId.GetHashCode() ?? 0xffU) ^ 0xab73f0;
                    Drawing.DrawRect(Main.spriteBatch, rect, new Color { PackedValue = color });

                }
                Main.spriteBatch.End();
            }

            ModContent.GetInstance<ComponentInterface>().Draw();
        }

        public bool DrawUI()
        {
            ProgrammerInterface.Draw();
            StatInfo.Draw();
            FloatingText.Draw(Main.spriteBatch);

            Point mouse = (Main.MouseWorld / 16).ToPoint();

            Tile t = Main.tile[mouse.X, mouse.Y];

            ComponentData data = GetDataOrNull(new(mouse.X, mouse.Y));
            if (data is not null && data.LastErrors.Count > 0)
            {
                AddHoverText(Util.ColorTag(Color.OrangeRed, $"Errors:\n{string.Join('\n', data.LastErrors)}"));
            }

            if (TerraIntegration.DebugMode && ComponentDebug)
                AddHoverText($"X: {mouse.X} Y: {mouse.Y}\nFx: {t.TileFrameX} Fy: {t.TileFrameY}\n{TileLoader.GetTile(t.TileType)?.Name}");

            if (HoverItem is not null)
            {
                ItemSlot.MouseHover(ref HoverItem, 0);
                HoverItem = null;
            }
            if (HoverText is not null)
            {
                Main.instance.MouseText(HoverText);
                HoverText = null;
            }
            return true;
        }

        public override void UpdateUI(GameTime gameTime)
        {
            VariableHighlights.Clear();

            ProgrammerInterface.Update();
            ModContent.GetInstance<ComponentInterface>().Update();
            FloatingText.Update();
        }

        public override void SaveWorldData(TagCompound tag)
        {
            List<TagCompound> components = new();

            foreach (ComponentData data in EnumerateAllComponentData())
            {
                bool shouldSave = false;
                if (data.Component is not null)
                    shouldSave = data.Component.ShouldSaveData(data);
                if (data is UnloadedComponentData)
                    shouldSave = true;
                else if (data is SubTileComponentData)
                    shouldSave = false;

                if (!shouldSave) continue;

                TagCompound t = data is UnloadedComponentData ?
                    UnloadedComponent.SaveTag(data) :
                    data.Component.SaveTag(data);
                if (t is null) continue;
                t["x"] = data.Position.X;
                t["y"] = data.Position.Y;
                components.Add(t);
            }

            tag["components"] = components;
        }
        public override void LoadWorldData(TagCompound tag)
        {
            if (tag.ContainsKey("components"))
            {
                IList<TagCompound> components = tag.GetList<TagCompound>("components");

                foreach (TagCompound component in components)
                {
                    Point16 pos = new();

                    if (component.ContainsKey("x"))
                        pos.X = component.GetShort("x");
                    if (component.ContainsKey("y"))
                        pos.Y = component.GetShort("y");

                    ComponentData data = Component.LoadTag(component, pos);
                    if (data is null || !Framing.GetTileSafely(pos).HasTile) continue;

                    SetData(pos, data);
                }
            }
        }

        public void HighlightItem(SpriteBatch spriteBatch, Item item, Texture2D back, Vector2 position, float scale)
        {
            if (item.type != ModContent.ItemType<Items.Variable>()) return;
            Items.Variable var = item.ModItem as Items.Variable;

            if (var?.Var is null) return;

            bool needsHighlight = VariableHighlights.Any(h => h(var.Var));
            if (!needsHighlight && var.Highlight == 0) return;

            if (needsHighlight && var.Highlight < 255)
                var.Highlight = (byte)Math.Min(255, var.Highlight + 16);
            else if (!needsHighlight && var.Highlight > 0)
                var.Highlight = (byte)Math.Max(0, var.Highlight - 16);

            Vector2 backSize = back.Size() * scale;
            Vector2 hlScale = backSize / HighlightTexture.Size();

            spriteBatch.Draw(HighlightTexture, position, null, Color.White * (var.Highlight / 255f), 0f, Vector2.Zero, hlScale, SpriteEffects.None, 0);
        }

        public override void PostWorldGen()
        {
            Bluewood tree = ModContent.GetInstance<Bluewood>();
            HashSet<Point> foundValidTrees = new();


            for (int i = 1; i < Main.maxTilesX - 1; i++)
                for (int j = 1; j < Main.worldSurface; j++)
                    if (TileID.Sets.IsATreeTrunk[Main.tile[i, j].TileType])
                    {
                        TreeTileInfo info = TreeTileInfo.GetInfo(i, j);
                        if (!info.IsCenter) continue;

                        TreeStats stats = TreeGrowing.GetTreeStats(i, j);

                        if (stats.Bottom.Y > 0)
                            j = stats.Bottom.Y;

                        if (!tree.ValidGroundType(stats.GroundType)) continue;

                        foundValidTrees.Add(stats.Bottom);
                    }
            int treesToSpawn = Main.maxTilesX / 200;

            treesToSpawn = Math.Max(5, Math.Min(treesToSpawn, foundValidTrees.Count / 5));

            List<Point> validTrees = new(foundValidTrees);

            while (treesToSpawn > 0 && validTrees.Count > 0)
            {
                int index = WorldGen.genRand.Next(validTrees.Count);
                Point pos = validTrees[index];

                foreach (PositionedTreeTile tile in TreeGrowing.EnumerateTreeTiles(pos.X, pos.Y))
                    Main.tile[tile.Pos.X, tile.Pos.Y].ClearTile();

                if (TreeGrowing.GrowTree(pos.X, pos.Y + 1, tree.GetTreeSettings()))
                    treesToSpawn--;

                validTrees.RemoveAt(index);
            }
        }

        internal void ResetHoverText()
        {
            HoverText = null;
        }

        public void AddHoverText(string text)
        {
            if (!HoverText.IsNullEmptyOrWhitespace())
                HoverText += "\n" + text;
            else
                HoverText = text;
        }
    }

    public class ComponentUITab
    {
        public string Name { get; set; }
        public Func<PositionedComponent, bool> IsAvailable { get; set; }
        public Action<Vector2> Setup { get; set; }

        public ComponentUITab(string name, Func<PositionedComponent, bool> isAvailable, Action<Vector2> setup)
        {
            Name = name;
            IsAvailable = isAvailable;
            Setup = setup;
        }
    }

    public class DelayedWireTrip
    {
        public int Delay, X, Y, Width, Height;

        public DelayedWireTrip(int x, int y, int width, int height, int delay)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
            Delay = delay;
        }
    }
}
