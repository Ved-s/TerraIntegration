﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerraIntegration.Components;
using TerraIntegration.Variables;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;

namespace TerraIntegration.UI
{
    public class ComponentInterface : ILoadable
    {
        public UserInterface Interface = new();
        public PositionedComponent InterfaceComponent;
        public Vector2 InterfaceOffset = default;
        public int CurrentTab;
        public List<ComponentUITab> Tabs = new();


        private int ReloadButtonHeight = 30;
#if DEBUG
        private bool ShowUIReload = true;
#else
        private bool ShowUIReload = false;
#endif

        public void Load(Mod mod)
        {
            Tabs.Add(new ComponentUITab("Interface",
                    () => true,
                    SetupInterface));

            Tabs.Add(new ComponentUITab("Properties",
                () => PropertyVariable.ByComponentType.ContainsKey(InterfaceComponent.Component.ComponentType),
                SetupProperties));

            Tabs.Add(new ComponentUITab("Config",
                () => InterfaceComponent.Component.DefaultUpdateFrequency > 0,
                SetupComponentConfig));
        }
        public void Unload()
        {
            Tabs.Clear();
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
                InterfaceComponent.Component.SetUpdates(InterfaceComponent.Pos, (ushort)Math.Max(0, s + data.UpdateFrequency));
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
                ComponentUITab tab = Tabs[i];
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
        
        public void CloseUI()
        {
            InterfaceComponent = default;
            Interface.CurrentState.Deactivate();
            SoundEngine.PlaySound(SoundID.MenuClose);
            return;
        }

        public void Draw()
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
        public void Update() 
        {
            if (InterfaceComponent.Component is not null)
            {
                Interface.Update(Main.gameTimeCache);
                if (Interface.CurrentState.IsMouseHovering)
                    Main.LocalPlayer.mouseInterface = true;
            }
        }
    }
}
