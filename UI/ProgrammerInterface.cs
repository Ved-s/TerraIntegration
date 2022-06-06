using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using TerraIntegration.Basic;
using TerraIntegration.Values;
using TerraIntegration.Variables;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.Localization;
using Terraria.UI;

namespace TerraIntegration.UI
{
    public static class ProgrammerInterface
    {
        public static UserInterface Interface = new();
        public static UIState State = new();

        public static bool Active = false;

        static bool BackGrab = false;
        static bool BackResizeW = false;
        static bool BackResizeH = false;
        static Vector2 BackGrabPos;
        static Vector2 InterfacePos = new(500, 320);
        static Vector2 InterfaceSize;

        static UIPanel Back;
        static UITextPanel<string> Reload;

        static UIText VariablesText;
        static UIFocusInputTextField VariablesSearch;
        static UIPanel Variables;
        static UIList VariablesList;
        static UIScrollbar VariablesScroll;

        static UIText PropertiesText;
        static UIFocusInputTextField PropertiesSearch;
        static UIPanel Properties;
        static UIList PropertiesList;
        static UIScrollbar PropertiesScroll;

        static UIPanel VariableInterface;
        static UIPanel ProgrammerOutput;

        static UITextPanel<string> WriteButton;
        static UIText ResultText;
        static UIVariableSlot ResultSlot;

        static IOwnProgrammerInterface CurrentOwner;

        static VariableValue CurrentValue;
        static Type CurrentType;

        static UIFocusInputTextField VariableName;
        static UIPanel SelectedPanel;
        static Color PreviousSelectedColor;

        static ProgrammerInterface()
        {
            Interface.SetState(State);
            SetupUI();
            State.Deactivate();
        }

        public static void SetupUI()
        {
            if (InterfaceSize == default)
            {
                InterfaceSize = new(Main.screenWidth - 700, Main.screenHeight - 420);
            }

            State.Deactivate();
            State.RemoveAllChildren();
            State.Activate();

            State.Append(Back = new()
            {
                Top = new(InterfacePos.Y, 0),
                Left = new(InterfacePos.X, 0),
                Width = new(InterfaceSize.X, 0),
                Height = new(InterfaceSize.Y, 0),

                BackgroundColor = new Color(0x4e, 0x48, 0x64) * 0.8f,
            });

            Back.Append(VariablesText = new("Variables")
            {
                Width = new(-10, 0.25f),
                MarginTop = 10,
                MarginLeft = 10,
            });
            Back.Append(Variables = new()
            {
                Width = new(-30, 0.25f),
                Height = new(-70, 1),

                MarginTop = 60,
                MarginLeft = 10,
                MarginRight = 0,
                MarginBottom = 0,

                PaddingTop = 5,
                PaddingLeft = 5,
                PaddingRight = 5,
                PaddingBottom = 5,

                BackgroundColor = new Color(0x48, 0x51, 0x64, 200),
            });
            Back.Append(VariablesSearch = new("Search")
            {
                Width = new(-10, 0.25f),
                Height = new(25, 0),

                MarginTop = 30,
                MarginLeft = 10,
                MarginRight = 0,
                MarginBottom = 0,

                PaddingTop = 3,
                PaddingLeft = 3,
                PaddingRight = 0,
                PaddingBottom = 0,

                BackgroundColor = new Color(0x48, 0x63, 0x64, 200),
            });
            Variables.Append(VariablesList = new()
            {
                Width = new(0, 1),
                Height = new(0, 1),
                ManualSortMethod = (i) => { }
            });
            Back.Append(VariablesScroll = new()
            {
                Top = new(66, 0),
                Left = new(-20, 0.25f),
                Width = new(20, 0),
                Height = new(-82, 1),
            });

            Back.Append(PropertiesText = new("Properties")
            {
                Left = new(0, .25f),
                Width = new(-10, 0.25f),
                MarginTop = 10,
                MarginLeft = 5,
            });
            Back.Append(Properties = new()
            {
                Left = new(0, .25f),
                Width = new(-30, 0.25f),
                Height = new(-70, 1),

                MarginTop = 60,
                MarginLeft = 5,
                MarginRight = 0,
                MarginBottom = 0,

                PaddingTop = 5,
                PaddingLeft = 5,
                PaddingRight = 5,
                PaddingBottom = 5,

                BackgroundColor = new Color(0x48, 0x51, 0x64, 200),
            });
            Back.Append(PropertiesSearch = new("Search")
            {
                Left = new(0, .25f),
                Width = new(-10, 0.25f),
                Height = new(25, 0),

                MarginTop = 30,
                MarginLeft = 5,
                MarginRight = 0,
                MarginBottom = 0,

                PaddingTop = 3,
                PaddingLeft = 3,
                PaddingRight = 0,
                PaddingBottom = 0,

                BackgroundColor = new Color(0x48, 0x63, 0x64, 200),
            });
            Properties.Append(PropertiesList = new()
            {
                Width = new(0, 1),
                Height = new(0, 1),
                ManualSortMethod = (i) => { }
            });
            Back.Append(PropertiesScroll = new()
            {
                Top = new(66, 0),
                Left = new(-25, 0.5f),
                Width = new(20, 0),
                Height = new(-82, 1),
            });

            CurrentOwner?.SetupInterface();
            SetInterface(CurrentOwner);

            Back.Append(VariableName = new("Variable name")
            {
                Top = new(-115, 1),
                Left = new(0, .5f),
                Width = new(-10, .5f),
                Height = new(25, 0),

                PaddingTop = 3,
                PaddingLeft = 3,
                PaddingRight = 0,
                PaddingBottom = 0,

                BackgroundColor = new Color(0x47, 0x8c, 0x9f, 200),
            });
            Back.Append(ProgrammerOutput = new()
            {
                Top = new(-85, 1),
                Left = new(0, .5f),
                Width = new(-10, .5f),
                Height = new(75, 0),

                PaddingTop = 0,
                PaddingLeft = 0,
                PaddingRight = 0,
                PaddingBottom = 0,

                BackgroundColor = new Color(0x3b, 0x62, 0x8a, 200),
            });

            ProgrammerOutput.Append(WriteButton = new("Write")
            {
                Top = new(20, 0),
                Left = new(20, 0),
                Width = new(-10, .3f),
                Height = new(-40, 1),

                BackgroundColor = new Color(0x63, 0xa1, 0x91, 200),
            });
            ProgrammerOutput.Append(ResultSlot = new()
            {
                Top = new(-18, .5f),
                Left = new(-56, 1),

                AcceptEmpty = true
            });
            ProgrammerOutput.Append(ResultText = new("Result:")
            {
                Top = new(-6, .5f),
                Left = new(-120, 1),
            });

            if (TerraIntegration.DebugMode)
            {
                State.Append(Reload = new UITextPanel<string>("Reload", 0.8f, false)
                {
                    Top = new(0, 0),
                    Left = new(0, 0),
                    Width = new(60, 0),
                    Height = new(25, 0),

                    BackgroundColor = Color.Yellow,

                    PaddingTop = 5,
                    PaddingLeft = 0,
                    PaddingRight = 0,
                    PaddingBottom = 0,
                    MarginTop = 0,
                    MarginLeft = 0,
                    MarginRight = 0,
                    MarginBottom = 0,
                });
                Reload.OnClick += (s, e) =>
                {
                    if (CurrentOwner is not null)
                    {
                        CurrentOwner.Interface = null;
                        CurrentOwner.SetupInterfaceIfNeeded();
                        VariableInterface = CurrentOwner.Interface;
                    }

                    SetupUI();
                    SoundEngine.PlaySound(SoundID.MenuTick);
                };
            }

            Back.SetPadding(0);
            Back.OnMouseDown += (ev, el) =>
            {
                if (ev.Target != Back) return;

                BackGrab = true;
                CalculatedStyle s = Back.GetDimensions();
                BackGrabPos = ev.MousePosition - s.Position();

                Vector2 endPos = new Vector2(s.Width, s.Height) - BackGrabPos;
                if (endPos.X <= 10 || endPos.Y <= 10)
                {
                    BackGrab = false;
                    BackGrabPos = endPos;

                    BackResizeW = endPos.X <= 10;
                    BackResizeH = endPos.Y <= 10;
                }
            };
            Back.OnMouseUp += (ev, el) =>
            {
                BackGrab = false;
                BackResizeW = false;
                BackResizeH = false;
            };
            WriteButton.OnClick += WriteButton_OnClick;

            VariablesSearch.OnTextChange += PopulateVariables;
            VariablesList.SetScrollbar(VariablesScroll);

            PropertiesSearch.OnTextChange += PopulateProperties;
            PropertiesList.SetScrollbar(PropertiesScroll);

            ResultSlot.VariableChanged += ResultSlot_VariableChanged;

            PopulateVariables();
        }

        private static void ResultSlot_VariableChanged(Items.Variable var)
        {
            if (var?.Var is null)
            {
                VariableName.SetText("");
            }
            else if (var.Var.Name is not null)
            {
                VariableName.SetText(var.Var.Name);
            }
        }

        private static void WriteButton_OnClick(UIMouseEvent evt, UIElement listeningElement)
        {
            if (ResultSlot?.Var is null) return;
            if (CurrentOwner is not null)
            {
                Guid id = ResultSlot.Var.Var is null ? default : ResultSlot.Var.Var.Id;
                Variable result = CurrentOwner.WriteVariable();
                if (result is not null)
                {
                    ResultSlot.Var.Var = result;
                    if (id != default)
                        ResultSlot.Var.Var.Id = id;
                }
            }

            if (ResultSlot.Var?.Var is not null && !VariableName.CurrentString.IsNullEmptyOrWhitespace())
            {
                ResultSlot.Var.Var.Name = VariableName.CurrentString;
            }

            SoundEngine.PlaySound(SoundID.MenuTick);
        }

        public static void Draw()
        {
            if (!Main.playerInventory || !Active) return;

            Interface.Draw(Main.spriteBatch, Main.gameTimeCache);
        }
        public static void Update()
        {
            if (!Main.playerInventory || !Active)
            {
                if (Active) State.Deactivate();
                Active = false;
                return;
            }

            CalculatedStyle back = Back.GetDimensions();
            if (BackGrab)
            {
                Vector2 pos = Main.MouseScreen - BackGrabPos;

                if (pos.X < 0)
                    pos.X = 0;
                if (pos.Y < 0)
                    pos.Y = 0;
                if (pos.X + back.Width > Main.screenWidth)
                    pos.X = Main.screenWidth - back.Width;
                if (pos.Y + back.Height > Main.screenHeight)
                    pos.Y = Main.screenHeight - back.Height;

                BackGrabPos = Main.MouseScreen - pos;

                Back.Top = new(pos.Y, 0);
                Back.Left = new(pos.X, 0);
                InterfacePos = pos;
                Back.Recalculate();
                back = Back.GetDimensions();
            }
            if (BackResizeW || BackResizeH)
            {
                Vector2 size = new(back.Width, back.Height);
                Vector2 newsize = Main.MouseScreen + BackGrabPos - back.Position();
                if (BackResizeW) size.X = newsize.X;
                if (BackResizeH) size.Y = newsize.Y;

                Back.Width = new(size.X, 0);
                Back.Height = new(size.Y, 0);
                Back.Recalculate();
            }

            if (TerraIntegration.DebugMode)
            {
                Reload.Top = new(back.Y - Reload.Height.Pixels - 5, 0);
                Reload.Left = new(back.X + back.Width - Reload.Width.Pixels, 0);
                if (Reload.IsMouseHovering) Main.LocalPlayer.mouseInterface = true;
            }
            Interface.Update(Main.gameTimeCache);

            if (Back.IsMouseHovering)
                Main.LocalPlayer.mouseInterface = true;

            ResultSlot.WorldPos = Main.LocalPlayer.Center.ToPoint();
        }
        public static void Show()
        {
            Main.playerInventory = true;
            Active = true;
            State.Activate();
        }
        public static void Toggle()
        {
            if (Active)
            {
                State.Deactivate();
                Active = false;
            }
            else Show();
        }

        static void PopulateVariables()
        {
            VariablesList.Clear();
            PropertiesList.Clear();

            Select(null);

            foreach (var kvp in VariableValue.ByType)
            {
                VariableValue v = kvp.Value;
                if (!v.TypeName.ToLower().Contains(VariablesSearch.CurrentString.ToLower())
                    || (!v.HasProperties() && v is not IOwnProgrammerInterface)) continue;

                VariablesList.Add(CreateVariableButton(
                    v.TypeDefaultDisplayName,
                    v.TypeColor,
                    kvp.Key,
                    "const",
                    () => ValueClicked(v),
                    iconHoverText: v.TypeDescription));
            }

            foreach (Type t in ValueProperty.ByValueType.Keys)
            {
                if (!t.IsInterface || !t.Name.ToLower().Contains(VariablesSearch.CurrentString.ToLower())) continue;

                VariablesList.Add(CreateVariableButton(VariableValue.TypeToName(t, true), Color.White, t, null, () => TypeClicked(t)));
            }

            foreach (Variable v in Variable.ByTypeName.Values)
            {
                if (v is not IOwnProgrammerInterface interfaceOwner 
                    || !v.TypeDisplayName.ToLower().Contains(VariablesSearch.CurrentString.ToLower())
                    || !v.VisibleInProgrammerVariables) continue;

                VariablesList.Add(CreateVariableButton(v.TypeDisplayName, Color.White, v.VariableReturnType, v.TypeName, () => VariableClicked(interfaceOwner), iconHoverText: v.TypeDescription));
            }
        }
        static void PopulateProperties()
        {
            PropertiesList.Clear();

            if (CurrentType is not null)
            {
                List<Type> related = new() { CurrentType };

                if (CurrentType is not null)
                    related.Add(typeof(Constant));

                foreach (var (type, var) in Variable
                    .GetRelated(related)
                    .OrderBy(
                    v => v.Item1 == CurrentType && v.Item2 is ValueProperty,
                    v => v.Item1.IsSubclassOf(typeof(Variable)),
                    v => v.Item2 is ValueProperty && v.Item2 is not ValueConversion,
                    v => v.Item1 == CurrentType && v.Item2 is not ValueConversion,
                    v => v.Item2 is not ValueConversion))
                {
                    if (!var.TypeDisplayName.ToLower().Contains(PropertiesSearch.CurrentString.ToLower())
                        || CurrentValue is not null && var is ValueProperty prop && !prop.AppliesTo(CurrentValue)
                        || var is not IOwnProgrammerInterface owner) continue;

                    string headText = null;

                    if (type != CurrentType && !type.IsSubclassOf(typeof(Variable)))
                        headText = $"from {VariableValue.TypeToName(type, true)}";

                    UITextPanel panel = CreateVariableButton(var.TypeDisplayName, Color.White, var.VariableReturnType, var.TypeName, () => PropertyClicked(owner), headText);

                    PropertiesList.Add(panel);
                }
            }
        }

        static void ValueClicked(VariableValue value)
        {
            CurrentValue = value;
            CurrentType = value.GetType();
            SetInterface(value as IOwnProgrammerInterface);
            PopulateProperties();
        }
        static void TypeClicked(Type type)
        {
            CurrentValue = null;
            CurrentType = type;
            SetInterface(null);
            PopulateProperties();
        }
        static void VariableClicked(IOwnProgrammerInterface var)
        {
            CurrentValue = null;
            CurrentType = var.GetType();
            SetInterface(var);
            PopulateProperties();
        }

        static void PropertyClicked(IOwnProgrammerInterface prop)
        {
            SetInterface(prop);
        }

        static void SetInterface(IOwnProgrammerInterface owner)
        {
            CurrentOwner = owner;

            owner?.SetupInterfaceIfNeeded();

            if (VariableInterface is not null)
                Back.RemoveChild(VariableInterface);

            if (owner?.Interface is null)
            {
                VariableInterface = new();
            }
            else VariableInterface = owner.Interface;

            VariableInterface.Left = new(0, .5f);
            VariableInterface.Width = new(-10, .5f);
            VariableInterface.Height = new(-180, 1);
            VariableInterface.MarginTop = 60;
            VariableInterface.BackgroundColor = new Color(0x33, 0x61, 0x6e, 200);

            Back.Append(VariableInterface);
        }

        static void Select(UIPanel panel) 
        {
            if (SelectedPanel is not null)
                SelectedPanel.BackgroundColor = PreviousSelectedColor;

            if (panel is null) 
                return;

            SelectedPanel = panel;
            PreviousSelectedColor = panel.BackgroundColor;

            panel.BackgroundColor = new Color(100, 160, 180);
        }

        static UITextPanel CreateVariableButton(string text, Color color, Type returnValue, string variableType, Action click, string headText = null, string iconHoverText = null) 
        {
            UITextPanel panel = new UITextPanel(text, color)
            {
                Width = new(0, 1),
                Height = new(40, 0),

                MarginTop = 0,
                MarginLeft = 0,
                MarginRight = 0,
                MarginBottom = 0,

                PaddingLeft = 38,
                PaddingRight = 8,
                PaddingTop = 0,
                PaddingBottom = 0,
            };
            UIDrawing icon = new UIDrawing()
            {
                Top = new(4, 0),
                Left = new(-34, 0),
                Width = new(32, 0),
                Height = new(32, 0),
                OnDraw = (e, sb, st) => VariableRenderer.DrawVariableOverlay(sb, true, returnValue, variableType, st.Position(), new(32), Color.White, 0f, Vector2.Zero),
                HoverText = iconHoverText
            };
            panel.Append(icon);

            panel.OnClick += (ev, el) =>
            {
                Select(panel);
                SoundEngine.PlaySound(SoundID.MenuTick);
                click?.Invoke();
            };

            if (headText is not null)
            {
                panel.Height = new(46, 0);
                panel.PaddingTop = 6;
                icon.Top.Pixels -= 3;

                panel.Append(new UIText(headText, .7f)
                {
                    Top = new(-3, 0),
                    Left = new(3, 0)
                });
            }

            return panel;
        }
    }
}
