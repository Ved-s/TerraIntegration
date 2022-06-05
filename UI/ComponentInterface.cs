using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerraIntegration.Basic;
using TerraIntegration.ComponentProperties;
using TerraIntegration.Components;
using TerraIntegration.DataStructures;
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

        public void Load(Mod mod)
        {
            Tabs.Add(new ComponentUITab("Interface",
                    () => InterfaceComponent.Component.HasCustomInterface,
                    SetupInterface));

            Tabs.Add(new ComponentUITab("Properties",
                () => InterfaceComponent.Component.HasProperties(),
                SetupProperties));

            Tabs.Add(new ComponentUITab("Variables",
                () => InterfaceComponent.Component.VariableInfo?.Length is not null and > 0,
                SetupVariables));

            Tabs.Add(new ComponentUITab("Description",
                () => !InterfaceComponent.Component.ComponentDescription.IsNullEmptyOrWhitespace(),
                SetupDescription));

            Tabs.Add(new ComponentUITab("Config",
                () => InterfaceComponent.Component.DefaultUpdateFrequency > 0 
                && InterfaceComponent.Component.ConfigurableFrequency,
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
            CurrentTab = 0;
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

        private void SetupComponentName()
        {
            if (InterfaceComponent.Component.ComponentDisplayName is null) return;

            UIState s = Interface.CurrentState;

            UIText text = new UIText(InterfaceComponent.Component.ComponentDisplayName)
            {
                Left = new(6, 0)
            };

            MoveUI(0, text.MinHeight.Pixels);

            s.Append(text);
        }
        private void SetupReloadButton()
        {
            if (!TerraIntegration.DebugMode) return;

            UIState s = Interface.CurrentState;

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

        private void SetupInterface(Vector2 pos)
        {
            UIPanel p = InterfaceComponent.Component.Interface;
            p.Top = new(pos.Y, 0);
            p.Left = new(pos.X, 0);

            Interface.CurrentState.Append(p);
        }
        private void SetupProperties(Vector2 pos)
        {
            IEnumerable<ComponentProperty> props = InterfaceComponent.Component.GetProperties();
            if (props is not null)
            {
                int height = 0;

                SetMinUISize(250);

                UIList list = new()
                {
                    Width = new(0, 1),
                    Height = new(247, 0),

                    Top = new(pos.Y, 0),
                    Left = new(pos.X, 0),
                };

                foreach (ComponentProperty v in props)
                {
                    var def = new UIComponentVariableDefinition(v, InterfaceComponent)
                    {
                        Width = new(0, 1),

                        DefineVariable = (var) => var.Var = v.CreateVariable(InterfaceComponent)
                    };
                    list.Add(def);

                    height += (int)(def.Height.Pixels) + 4;
                }

                Interface.CurrentState.Append(list);

                if (height > list.Height.Pixels)
                {
                    list.Width = new(-22, 1);

                    UIScrollbar sb = new()
                    {
                        Height = new(list.Height.Pixels - 12, 0),
                        Top = new(pos.Y + 6, 0),
                        Left = new(-20, 1),
                    };
                    list.SetScrollbar(sb);
                    Interface.CurrentState.Append(sb);
                }
            }
        }
        private void SetupVariables(Vector2 pos)
        {
            ComponentVariableInfo[] info = InterfaceComponent.Component.VariableInfo;
            if (info is null || info.Length == 0) return;

                int y = (int)pos.Y;

                foreach (ComponentVariableInfo inf in info)
                {
                    var var = new UIComponentNamedVariable()
                    {
                        Top = new(y, 0),
                        Left = new(pos.X, 0),
                        Width = new(0, 1),

                        VariableTypes = inf.AcceptVariableTypes,
                        VariableReturnTypes = inf.AcceptVariableReturnTypes,
                        VariableName = inf.VariableName,
                        VariableSlot = inf.VariableSlot,
                        VariableDescription = inf.VariableDescription,
                        Component = InterfaceComponent
                    };
                    Interface.CurrentState.Append(var);

                    y += (int)(var.Height.Pixels) + 4;
                }
            
        }
        private void SetupDescription(Vector2 pos) 
        {
            if (InterfaceComponent.Component.ComponentDescription.IsNullEmptyOrWhitespace()) return;

            float height = 32f * (InterfaceComponent.Component.ComponentDescription.Count(c => c == '\n') + 1);

            Interface.CurrentState.Append(new UITextPanel<string>(InterfaceComponent.Component.ComponentDescription)
            {
                MinWidth = new(200, 0),
                Width = new(0, 1),
                Height = new(height, 0),
                Top = new(pos.Y, 0),
                Left = new(pos.X, 0),
            });
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

        public void SetupTabSwitch()
        {
            int tabs = GetTabsAvailable();
            if (tabs == 0) return;

            int x = 0;

            List<UITextPanel<string>> uiTabs = new();

            for (int i = 0; i < Tabs.Count; i++)
            {
                ComponentUITab tab = Tabs[i];
                if (!tab.IsAvailable())
                    continue;

                int tabind = i;

                UITextPanel<string> tabpanel = new(tab.Name)
                {
                    Top = new(4, 0),
                    Left = new(x, 0),
                    Height = new(24, 0),

                    PaddingTop = 5,
                    PaddingBottom = 0,

                };
                if (i == CurrentTab)
                    tabpanel.BackgroundColor = new Color(100, 160, 180);

                tabpanel.OnClick += (s, e) =>
                {
                    CurrentTab = tabind;
                    SoundEngine.PlaySound(SoundID.MenuTick);
                    SetupUI();
                };
                Interface.CurrentState.Append(tabpanel);
                uiTabs.Add(tabpanel);

                x += (int)tabpanel.MinWidth.Pixels + 4;
            }

            x -= 4;
            SetMinUISize(x);
            foreach (var tab in uiTabs)
            {
                tab.Width = new(0, tab.MinWidth.Pixels / x);
                tab.Left = new(0, tab.Left.Pixels / x);
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

            s.MinWidth = StyleDimension.Empty;
            s.MinHeight = StyleDimension.Empty;

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
            SetupComponentName();
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
        private void SetMinUISize(float? width = null, float? height = null)
        {
            if (width is not null)
                Interface.CurrentState.MinWidth = new(Math.Max(width.Value, Interface.CurrentState.MinWidth.Pixels), 0);

            if (height is not null)
                Interface.CurrentState.MaxHeight = new(Math.Max(height.Value, Interface.CurrentState.MaxHeight.Pixels), 0);
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

                Vector2 screen = Util.WorldPixelsToScreen(InterfaceComponent.Pos.ToVector2() * 16 + InterfaceComponent.Component.InterfaceOffset);

                screen /= Main.UIScale;
                screen += InterfaceOffset;
                

                //screen *= Main.GameZoomTarget;
                //screen /= Main.UIScale;

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
                {
                    UIElement e = Interface.CurrentState.GetElementAt(Main.MouseScreen);

                    if (e is not null)
                    {
                        bool hasBlockingUI = false;
                        do
                        {
                            if (e is UIPanel || e is UIScrollbar)
                            {
                                hasBlockingUI = true;
                                break;
                            }
                            e = e.Parent;
                        }
                        while (e is not null && e.Parent is not null && e is not UIState);

                        Main.LocalPlayer.mouseInterface = hasBlockingUI;
                    }
                }
            }
        }
    }
}
