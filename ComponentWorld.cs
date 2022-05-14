using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using TerraIntegration.Components;
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

namespace TerraIntegration
{
    public class ComponentWorld : ModSystem
    {
        public new static TerraIntegration Mod => ModContent.GetInstance<TerraIntegration>();

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

        public T GetData<T>(Point16 pos, Component c = null) where T : ComponentData, new()
        {
            if (ComponentData.TryGetValue(pos, out ComponentData data))
            {
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
            newData.Init(c);
            if (data is not null)
                data.CopyTo(newData);

            ComponentData[pos] = newData;
            return newData;
        }
        public ComponentData GetData(Point16 pos, Component c = null)
        {
            if (ComponentData.TryGetValue(pos, out ComponentData data))
            {
                return data;
            }

            if (c is null)
            {
                Tile t = Main.tile[pos.X, pos.Y];
                if (!Component.ByTileType.TryGetValue(t.TileType, out c))
                    return null;
            }

            ComponentData newData = new();
            newData.Init(c);
            ComponentData[pos] = newData;
            return newData;
        }
        public T GetDataOrNull<T>(Point16 pos) where T : ComponentData
        {
            if (ComponentData.TryGetValue(pos, out ComponentData data) && data is T t)
            {
                return t;
            }
            return null;
        }
        public ComponentData GetDataOrNull(Point16 pos)
        {
            if (ComponentData.TryGetValue(pos, out ComponentData data))
            {
                return data;
            }

            return null;
        }
        
        public void RemoveAll(Point16 pos)
        {
            if (ComponentData.TryGetValue(pos, out ComponentData data))
            {
                data.Destroy(pos);
                ComponentData.Remove(pos);
            }
            ComponentUpdates.Remove(pos);
        }

        public override void Load()
        {
            if (!Main.dedServ)
                HighlightTexture = ModContent.Request<Texture2D>($"{Mod.Name}/Assets/UI/SlotHighlight", AssetRequestMode.ImmediateLoad).Value;
        }
        public override void OnWorldUnload()
        {
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
            Statistics.Start(Statistics.UpdateTime.Components);
            foreach (KeyValuePair<Point16, Component> kvp in ComponentUpdates)
            {
                ComponentData data = GetData(kvp.Key, kvp.Value);
                if (data.UpdateFrequency == 0) continue;
                if (UpdateCounter % data.UpdateFrequency == 0)
                {
                    kvp.Value.OnUpdate(kvp.Key);
                    Statistics.UpdatedComponents++;
                }
                
            }
            Statistics.Stop(Statistics.UpdateTime.Components);

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

            foreach (KeyValuePair<Point16, ComponentData> kvp in ComponentData)
            {
                bool shouldSave = false;
                if (kvp.Value.Component is not null)
                    shouldSave = kvp.Value.Component.ShouldSaveData(kvp.Value);
                if (kvp.Value is UnloadedComponentData)
                    shouldSave = true;

                if (!shouldSave) continue;

                components.Add(new()
                {
                    ["x"] = kvp.Key.X,
                    ["y"] = kvp.Key.Y,
                    ["data"] = kvp.Value is UnloadedComponentData ?
                    UnloadedComponent.SaveTag(kvp.Value) :
                    kvp.Value.Component.SaveTag(kvp.Value)
                });
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

                    if (!component.ContainsKey("data")) continue;
                    ComponentData data = Component.LoadTag(component.GetCompound("data"));

                    if (data is null) continue;

                    ComponentData[pos] = data;
                }
            }
        }

        public void HighlightItem(SpriteBatch spriteBatch, Item item, Texture2D back, Vector2 position, float scale)
        {
            if (item.type != ModContent.ItemType<Items.Variable>()) return;
            Items.Variable var = item.ModItem as Items.Variable;

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
