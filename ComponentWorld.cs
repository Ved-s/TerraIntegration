using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
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

        public UserInterface Interface = new();
        public PositionedComponent InterfaceComponent;
        public Vector2 InterfaceOffset = default;
        public int CurrentTab;
        public List<UITab> Tabs = new()
        {
            new UITab("Interface",
                () => true,
                (p) => ModContent.GetInstance<ComponentWorld>().SetupInterface(p)),

            new UITab(
                "Properties",
                () => PropertyVariable.ByComponentType.ContainsKey(ModContent.GetInstance<ComponentWorld>().InterfaceComponent.Component.ComponentType),
                (p) => ModContent.GetInstance<ComponentWorld>().SetupProperties(p)),

            new UITab("Config",
                () => ModContent.GetInstance<ComponentWorld>().InterfaceComponent.Component.DefaultUpdateFrequency > 0,
                (p) => ModContent.GetInstance<ComponentWorld>().SetupComponentConfig(p)),
        };

        public readonly HashSet<string> TypeHighlights = new();
        public readonly HashSet<Type> ReturnTypeHighlights = new();

        private int ReloadButtonHeight = 30;
#if DEBUG
        private bool ShowUIReload = true;
#else
        private bool ShowUIReload = false;
#endif

        private UnloadedComponent UnloadedComponent = new();

        public T GetData<T>(Point16 pos, Component c) where T : ComponentData, new()
        {
            if (ComponentData.TryGetValue(pos, out ComponentData data))
            {
                if (data is T tdata)
                    return tdata;
            }

            T newData = new();
            newData.Init(c);
            if (data is not null)
                data.CopyTo(newData);

            ComponentData[pos] = newData;
            return newData;
        }
        public ComponentData GetData(Point16 pos, Component c)
        {
            if (ComponentData.TryGetValue(pos, out ComponentData data))
            {
                return data;
            }

            ComponentData newData = new();
            newData.Init(c);
            ComponentData[pos] = newData;
            return newData;
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
                    kvp.Value.OnUpdate(kvp.Key);
                Statistics.UpdatedComponents++;
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
            if (InterfaceComponent.Component is not null)
            {
                Tile t = Main.tile[InterfaceComponent.Pos.X, InterfaceComponent.Pos.Y];

                if (!InterfaceComponent.Component.CheckShowInterface(InterfaceComponent.Pos)
                    || !CheckReach(InterfaceComponent.Component.GetInterfaceReachCheckPos(InterfaceComponent.Pos))
                    || !t.HasTile
                    || t.TileType != InterfaceComponent.Component.Type
                    )
                {
                    CloseUI();
                    return;
                }

                Main.spriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, null, null, Main.UIScaleMatrix);

                Vector2 screen = (InterfaceComponent.Pos.ToVector2() * 16) - Main.screenPosition;

                screen += InterfaceComponent.Component.InterfaceOffset;
                screen += InterfaceOffset;

                screen *= Main.GameZoomTarget;
                screen /= Main.UIScale;

                Interface.CurrentState.Top = new(screen.Y, 0);
                Interface.CurrentState.Left = new(screen.X, 0);
                Interface.Draw(Main.spriteBatch, Main.gameTimeCache);

                Main.spriteBatch.End();
            }
        }

        public void SetInterfaceComponent(Point16 pos, Component c)
        {
            if (!CheckReach(c.GetInterfaceReachCheckPos(pos))) return;

            if (InterfaceComponent.Pos == pos)
            {
                CloseUI();
                return;
            }

            InterfaceComponent = new(pos, c);
            if (Interface.CurrentState is null)
                Interface.SetState(new());
            SetupUI();

            SoundEngine.PlaySound(SoundID.MenuOpen);
        }

        private bool CheckReach(Vector2 pos)
        {
            Vector2 diff = pos - Main.LocalPlayer.Center;
            diff.X = Math.Abs(diff.X);
            diff.Y = Math.Abs(diff.Y);

            diff /= 16;
            return diff.X <= Main.LocalPlayer.lastTileRangeX && diff.Y <= Main.LocalPlayer.lastTileRangeY;
        }

        private void SetupReloadButton()
        {
            if (!ShowUIReload) return;

            UIState s = Interface.CurrentState;

            InterfaceOffset.Y -= ReloadButtonHeight;

            MoveUI(0, ReloadButtonHeight);

            var btn = new UITextPanel<string>("Reload", 0.8f, false)
            {
                Top = new(0, 0),
                Left = new(-60, 1),
                Width = new(60, 0),
                Height = new(ReloadButtonHeight - 5, 0),

                BackgroundColor = Color.Yellow,

                PaddingTop = 5,
                PaddingLeft = 0,
                PaddingRight = 0,
                PaddingBottom = 0,
                MarginTop = 0,
                MarginLeft = 0,
                MarginRight = 0,
                MarginBottom = 0,
            };
            btn.OnClick += (s, e) =>
            {
                InterfaceComponent.Component.ReloadInterface();
                SetupUI();
                SoundEngine.PlaySound(SoundID.MenuTick);
            };
            s.Append(btn);
        }
        private void SetupComponentConfig(Vector2 pos)
        {
            ComponentData data = InterfaceComponent.GetData();

            UIPanel panel = new()
            {
                Top = new(pos.Y, 0),
                Left = new(pos.X, 0),

                Width = new(0, 1),
                Height = new(32, 0),

                PaddingTop = 0,
                PaddingLeft = 0,
                PaddingRight = 0,
                PaddingBottom = 0,
            };
            UIText freq = new("Frequency: " + data.UpdateFrequency)
            {
                Top = new(8, 0),
                Left = new(8, 0),
                Height = new(20, 0),
            };
            UIStepButton freqStep = new()
            {
                Top = new(4, 0),
                Left = new(-28, 1),
            };

            freqStep.OnStep += (s) =>
            {
                ComponentData data = InterfaceComponent.GetData();
                data.UpdateFrequency = (ushort)Math.Max(0, s + data.UpdateFrequency);
                freq.SetText("Frequency: " + data.UpdateFrequency);
            };

            panel.Append(freq);
            panel.Append(freqStep);
            Interface.CurrentState.Append(panel);
        }
        private void SetupProperties(Vector2 pos)
        {
            IEnumerable<PropertyVariable> props = InterfaceComponent.Component.GetProperties();
            if (props is not null)
            {
                int y = (int)pos.Y;

                foreach (PropertyVariable v in props)
                {
                    var def = new UIComponentVariableDefinition()
                    {
                        Top = new(y, 0),
                        Left = new(pos.X, 0),
                        Width = new(0, 1),

                        VariableType = v.Type,
                        DefineVariable = (var) => var.Var = v.CreateVariable(InterfaceComponent)
                    };
                    Interface.CurrentState.Append(def);

                    y += (int)(def.Height.Pixels) + 4;
                }
            }
        }
        private void SetupInterface(Vector2 pos)
        {
            UIPanel p = InterfaceComponent.Component.Interface;
            p.Top = new(pos.Y, 0);
            p.Left = new(pos.X, 0);

            Interface.CurrentState.Append(p);
        }
        public void SetupTabSwitch()
        {
            if (GetTabsAvailable() < 2) return;

            int x = 0;
            for (int i = 0; i < Tabs.Count; i++)
            {
                UITab tab = Tabs[i];
                if (!tab.IsAvailable())
                    continue;

                int tabind = i;

                UITextPanel<string> tabpanel = new(tab.Name)
                {
                    Left = new(x, 0),
                    Height = new(24, 0),

                    PaddingTop = 5,
                    PaddingBottom = 0,
                };
                tabpanel.OnClick += (s, e) =>
                {
                    CurrentTab = tabind;
                    SetupUI();
                };
                Interface.CurrentState.Append(tabpanel);
                x += (int)tabpanel.MinWidth.Pixels + 4;
            }
        }

        public int GetTabsAvailable()
        {
            int tabs = 0;
            for (int i = 0; i < Tabs.Count; i++)
            {
                if (Tabs[i].IsAvailable())
                    tabs++;
            }
            return tabs;
        }

        private void SetupUI()
        {
            InterfaceComponent.Component.SetupInterfaceIfNeeded();
            InterfaceComponent.Component.UpdateInterface(InterfaceComponent.Pos);

            UIState s = Interface.CurrentState;

            s.RemoveAllChildren();

            s.PaddingTop = 0;
            s.PaddingLeft = 0;
            s.PaddingRight = 0;
            s.PaddingBottom = 0;

            if (CurrentTab >= Tabs.Count || !Tabs[CurrentTab].IsAvailable())
                for (int i = 0; i < Tabs.Count; i++)
                {
                    if (Tabs[i].IsAvailable())
                    {
                        CurrentTab = i;
                        break;
                    }
                }

            SetupTabSwitch();
            SetupReloadButton();

            GetMinUISize(out _, out float height);

            height += 4;

            InterfaceOffset = new(0, -height);

            Tabs[CurrentTab].Setup(new(0, height));
            FitUI();
            s.Activate();
        }
        public void MoveUI(float x, float y)
        {
            UIState s = Interface.CurrentState;
            s.Width.Pixels += x;
            s.Height.Pixels += y;

            foreach (UIElement child in s.Children)
            {
                child.Top.Pixels += y;
                child.Left.Pixels += x;
            }

        }
        public void FitUI()
        {
            float minWidth, minHeight;
            GetMinUISize(out minWidth, out minHeight);

            Interface.CurrentState.Width = new(minWidth, 0);
            Interface.CurrentState.Height = new(minHeight, 0);

            Interface.CurrentState.Recalculate();
        }
        private void GetMinUISize(out float minWidth, out float minHeight)
        {
            minWidth = 0;
            minHeight = 0;
            foreach (UIElement child in Interface.CurrentState.Children)
            {
                float childFitWidth = child.Left.Pixels + Math.Max(child.MinWidth.Pixels, child.Width.Pixels);
                float childFitHeight = child.Top.Pixels + Math.Max(child.MinHeight.Pixels, child.Height.Pixels);

                if (childFitWidth > minWidth)
                    minWidth = childFitWidth;
                if (childFitHeight > minHeight)
                    minHeight = childFitHeight;
            }
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
        public void CloseUI()
        {
            InterfaceComponent = default;
            Interface.CurrentState.Deactivate();
            SoundEngine.PlaySound(SoundID.MenuClose);
            return;
        }

        public override void UpdateUI(GameTime gameTime)
        {
            TypeHighlights.Clear();
            ReturnTypeHighlights.Clear();

            ProgrammerInterface.Update();
            if (InterfaceComponent.Component is not null)
            {
                Interface.Update(Main.gameTimeCache);
                if (Interface.CurrentState.IsMouseHovering)
                    Main.LocalPlayer.mouseInterface = true;
            }
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

            bool needsHighlight = TypeHighlights.Contains(var.Var.Type) || ReturnTypeHighlights.Contains(var.Var.VariableReturnType);
            if (!needsHighlight && var.Highlight == 0) return;

            if (needsHighlight && var.Highlight < 255)
                var.Highlight = (byte)Math.Min(255, var.Highlight + 16);
            else if (!needsHighlight && var.Highlight > 0)
                var.Highlight = (byte)Math.Max(0, var.Highlight - 16);

            Vector2 backSize = back.Size() * scale;
            Vector2 hlScale = backSize / HighlightTexture.Size();

            spriteBatch.Draw(HighlightTexture, position, null, Color.White * (var.Highlight / 255f), 0f, Vector2.Zero, hlScale, SpriteEffects.None, 0);
        }

        public class UITab
        {
            public string Name { get; set; }
            public Func<bool> IsAvailable { get; set; }
            public Action<Vector2> Setup { get; set; }

            public UITab(string name, Func<bool> isAvailable, Action<Vector2> setup)
            {
                Name = name;
                IsAvailable = isAvailable;
                Setup = setup;
            }
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
