using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerraIntegration.Components;
using TerraIntegration.UI;
using TerraIntegration.Variables;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent.UI.Elements;
using Terraria.Graphics;
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

        private UserInterface Interface = new();
        private PositionedComponent InterfaceComponent;
        private Vector2 InterfaceOffset = default;

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

        public override void OnWorldUnload()
        {
            ComponentData.Clear();
            Guids.Clear();
        }

        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
        {
            int ind = layers.FindIndex(l => l.Name == "Vanilla: Mouse Text");
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
            Interface.CurrentState.RemoveAllChildren();
            UIPanel interf = c.Interface;
            c.UpdateInterface(InterfaceComponent.Pos);
            SetupUI(interf);

            SoundEngine.PlaySound(SoundID.MenuOpen);
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

        private bool CheckReach(Vector2 pos)
        {
            Vector2 diff = pos - Main.LocalPlayer.Center;
            diff.X = Math.Abs(diff.X);
            diff.Y = Math.Abs(diff.Y);

            diff /= 16;
            return diff.X <= Main.LocalPlayer.lastTileRangeX && diff.Y <= Main.LocalPlayer.lastTileRangeY;
        }

        private void SetupUI(UIPanel p)
        {
            UIState s = Interface.CurrentState;
            InterfaceOffset = Vector2.Zero;

            s.PaddingTop = 0;
            s.PaddingLeft = 0;
            s.PaddingRight = 0;
            s.PaddingBottom = 0;

            p.Top = new(0, 0);
            p.Left = new(0, 0);

            s.Append(p);
            SetupReloadButton();

            IEnumerable<PropertyVariable> props = InterfaceComponent.Component.GetProperties();
            if (props is not null)
            {
                int y = (int)(p.Top.Pixels + p.Height.Pixels) + 8;

                s.Append(new UITextPanel<string>("Properties") 
                {
                    Top = new(y, 0),
                    Left = new(0, 0),
                    Width = new(250, 0),
                    Height = new(24, 0),

                    MarginTop = 0,
                    MarginBottom = 0,
                    PaddingTop = 4,
                    PaddingBottom = 0,
                });

                y += 28;

                foreach (PropertyVariable v in props)
                {
                    var def = new UIComponentVariableDefinition()
                    {
                        Top = new(y, 0),
                        Left = new(0, 0),
                        Width = new(0, 1),

                        VariableType = v.Type,
                        DefineVariable = (var) => var.Var = v.CreateVariable(InterfaceComponent)
                    };
                    s.Append(def);

                    y += (int)(def.Height.Pixels) + 4;
                }
            }

            FitUI();
            s.Activate();
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
                Interface.CurrentState.RemoveAllChildren();
                InterfaceComponent.Component.ReloadInterface();
                UIPanel interf = InterfaceComponent.Component.Interface;
                InterfaceComponent.Component.UpdateInterface(InterfaceComponent.Pos);
                SetupUI(interf);
                SoundEngine.PlaySound(SoundID.MenuTick);
            };
            s.Append(btn);
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
            float minWidth = 0;
            float minHeight = 0;

            foreach (UIElement child in Interface.CurrentState.Children)
            {
                float childFitWidth = child.Left.Pixels + child.Width.Pixels;
                float childFitHeight = child.Top.Pixels + child.Height.Pixels;

                if (childFitWidth > minWidth)
                    minWidth = childFitWidth;
                if (childFitHeight > minHeight)
                    minHeight = childFitHeight;
            }

            Interface.CurrentState.Width = new(minWidth, 0);
            Interface.CurrentState.Height = new(minHeight, 0);

            Interface.CurrentState.Recalculate();
        }

        public bool DrawUI() 
        {
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
