using CustomTreeLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using TerraIntegration.Basic;
using TerraIntegration.Components;
using TerraIntegration.DataStructures;
using TerraIntegration.UI;
using TerraIntegration.Variables;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.UI;
using Terraria.WorldBuilding;

namespace TerraIntegration
{
    public class ComponentWorld : ModSystem
    {
        public new static TerraIntegration Mod => ModContent.GetInstance<TerraIntegration>();
        public static ComponentWorld Instance => ModContent.GetInstance<ComponentWorld>();

        public ShortGuids Guids { get; } = new(2);

        public Dictionary<Point16, ComponentData> ComponentData = new();
        public Dictionary<Point16, Component> ComponentUpdates = new();

        public uint UpdateCounter = 0;

        public Item HoverItem = null;
        public string HoverText = null;

        public readonly List<DelayedWireTrip> WireTrips = new();

        private Texture2D HighlightTexture;

        public readonly List<VariableMatchDelegate> VariableHighlights = new();

        private UnloadedComponent UnloadedComponent = new();

        public T GetData<T>(Point16 pos, Component c = null, bool resolveSubTiles = true) where T : ComponentData, new()
        {
            if (ComponentData.TryGetValue(pos, out ComponentData data))
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

            if (c is null)
            {
                Tile t = Main.tile[pos.X, pos.Y];
                if (!Component.ByTileType.TryGetValue(t.TileType, out c))
                    return null;
            }

            T newData = new();
            newData.Init(c, pos);
            if (data is not null)
                data.CopyTo(newData);

            ComponentData[pos] = newData;
            return newData;
        }
        public ComponentData GetData(Point16 pos, Component c = null, bool resolveSubTiles = true)
        {
            if (ComponentData.TryGetValue(pos, out ComponentData data))
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

            ComponentData newData = new();
            newData.Init(c, pos);
            ComponentData[pos] = newData;
            return newData;
        }

        public T GetDataOrNull<T>(Point16 pos, bool resolveSubTiles = true) where T : ComponentData
        {
            if (ComponentData.TryGetValue(pos, out ComponentData data) && data is T t)
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
        public ComponentData GetDataOrNull(Point16 pos, bool resolveSubTiles = true)
        {
            if (ComponentData.TryGetValue(pos, out ComponentData data))
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

        public void InitData(Point16 pos, Component c) 
        {
            ComponentData newData = new();
            newData.Init(c, pos);
            ComponentData[pos] = newData;
        }
        public void InitData<T>(Point16 pos, Component<T> c) where T : ComponentData, new()
        {
            T newData = new();
            newData.Init(c, pos);
            ComponentData[pos] = newData;
        }

        public void DefineMultitile(Rectangle tileRect)
        {
            ComponentData data = GetData(new(tileRect.X, tileRect.Y));
            data.Size = new(tileRect.Width, tileRect.Height);

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
                    ComponentData[pos] = sub;
                }
        }

        public void RemoveAll(Point16 pos)
        {
            TileMimicking.MimicData.Remove(pos);
            if (ComponentData.TryGetValue(pos, out ComponentData data) && data.Position == pos)
            {
                data.Destroy(pos);
                ComponentData.Remove(pos);
            }
            ComponentUpdates.Remove(pos);
        }

        public bool HasData(Point16 pos)
        {
            if (ComponentData.TryGetValue(pos, out ComponentData data))
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
            foreach (var kvp in ComponentData)
            {
                if (kvp.Value is SubTileComponentData)
                    continue;
                
                yield return kvp.Value;
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
            Statistics.ResetUpdates();
            Statistics.Start(Statistics.UpdateTime.FullUpdate);

            Stopwatch watch = new();

            foreach (KeyValuePair<Point16, Component> kvp in ComponentUpdates)
            {
                ComponentData data = GetData(kvp.Key, kvp.Value);
                if (data.UpdateFrequency == 0) continue;
                if (UpdateCounter % data.UpdateFrequency == 0)
                {
                    Statistics.StartComponent(kvp.Value.TypeName);

                    //DateTime updateStart = DateTime.Now;
                    watch.Restart();
                    kvp.Value.OnUpdate(kvp.Key);
                    watch.Stop();
                    data.LastUpdateTime = watch.Elapsed;
                    Statistics.UpdatedComponents++;

                    Statistics.StopComponent(kvp.Value.TypeName);
                }
                
            }

            if (Main.netMode != NetmodeID.Server && !Main.LocalPlayer.mouseInterface)
            {
                Point16 mouseWorldTile = (Point16)(Main.MouseWorld / 16);

                if (ComponentData.TryGetValue(mouseWorldTile, out ComponentData cdata) && cdata.Component is not null)
                {
                    string hover = cdata.Component.GetHoverText(mouseWorldTile);
                    if (hover is not null)
                        HoverText = hover;
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
            Statistics.Stop(Statistics.UpdateTime.FullUpdate);
        }
        public override void PostDrawTiles()
        {
            ModContent.GetInstance<ComponentInterface>().Draw();
        }

        public bool DrawUI()
        {
            ProgrammerInterface.Draw();
            Statistics.Draw();

            //if (HoverText is null && TerraIntegration.DebugMode) 
            //{
            //    Point mouse = (Main.MouseWorld / 16).ToPoint();
            //
            //    //Tile t = Main.tile[mouse.X, mouse.Y];
            //    //
            //    //if (t.HasTile && TileID.Sets.IsATreeTrunk[t.TileType])
            //    //{
            //    //    TreeStats stats = TreeGrowing.GetTreeStats(mouse.X, mouse.Y);
            //    //    bool haveSettings = CustomTree.TryGetTreeSettingsByType(t.TileType, out TreeSettings settings);
            //    //
            //    //    HoverText = $"- TerraIntegration tree info -\ntX: {t.TileFrameX} tY: {t.TileFrameY}\n"
            //    //        + TreeTileInfo.GetInfo(mouse.X, mouse.Y) + "\n"
            //    //        + stats.ToString()
            //    //        + (haveSettings? 
            //    //            $"\nGV:{(settings.GroundTypeCheck(stats.GroundType)? "T" : "F")} " +
            //    //            $"GM:{(settings.CanGrowMoreCheck(stats.Top, settings, stats) ? "T" : "F")}" : "");
            //    //}
            //    //HoverText = mouse.ToString();
            //}

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
                    if (data is null) continue;

                    ComponentData[pos] = data;
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
                    if (TileID.Sets.IsATreeTrunk[Main.tile[i,j].TileType])
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

                if (TreeGrowing.GrowTree(pos.X, pos.Y+1, tree.GetTreeSettings()))
                    treesToSpawn--;

                validTrees.RemoveAt(index);
            }
        }
    }

    public class ComponentUITab
    {
        public string Name { get; set; }
        public Func<bool> IsAvailable { get; set; }
        public Action<Vector2> Setup { get; set; }

        public ComponentUITab(string name, Func<bool> isAvailable, Action<Vector2> setup)
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
