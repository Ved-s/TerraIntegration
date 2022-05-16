using Microsoft.Xna.Framework;
using TerraIntegration.Values;
using TerraIntegration.Variables;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
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
        static Vector2 InterfaceSize = new(740, 480);

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
        static UIFocusInputTextField VariableName;

        static ProgrammerInterface()
        {
            Interface.SetState(State);
            SetupUI();
            State.Deactivate();
        }

        public static void SetupUI()
        {
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

        private static void ResultSlot_VariableChanged()
        {
            if (ResultSlot.Var?.Var is null)
            {
                VariableName.SetText("");
            }
            else if (ResultSlot.Var.Var.Name is not null)
            {
                VariableName.SetText(ResultSlot.Var.Var.Name);
            }
        }

        private static void WriteButton_OnClick(UIMouseEvent evt, UIElement listeningElement)
        {
            if (ResultSlot?.Var is null) return;
            if (CurrentOwner is not null) CurrentOwner.WriteVariable(ResultSlot.Var);

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
            foreach (VariableValue v in VariableValue.ByTypeName.Values)
            {
                if (v is not IOwnProgrammerInterface interfaceOwner || !v.TypeDisplay.ToLower().Contains(VariablesSearch.CurrentString.ToLower())) continue;

                UITextPanel<string> panel = new UITextPanel<string>(v.TypeDisplay)
                {
                    Width = new(0, 1),
                    Height = new(30, 0),

                    MarginTop = 0,
                    MarginLeft = 0,
                    MarginRight = 0,
                    MarginBottom = 0,

                    TextColor = v.TypeColor,
                };
                panel.OnClick += (ev, el) =>
                {
                    SoundEngine.PlaySound(SoundID.MenuTick);
                    VariableClicked(interfaceOwner);
                };
                VariablesList.Add(panel);
            }

            foreach (Variable v in Variable.ByTypeName.Values)
            {
                if (v is not IOwnProgrammerInterface interfaceOwner || !v.TypeDisplay.ToLower().Contains(VariablesSearch.CurrentString.ToLower())) continue;

                UITextPanel<string> panel = new UITextPanel<string>(v.TypeDisplay)
                {
                    Width = new(0, 1),
                    Height = new(30, 0),

                    MarginTop = 0,
                    MarginLeft = 0,
                    MarginRight = 0,
                    MarginBottom = 0,
                };
                panel.OnClick += (ev, el) =>
                {
                    SoundEngine.PlaySound(SoundID.MenuTick);
                    VariableClicked(interfaceOwner);
                };
                VariablesList.Add(panel);
            }
        }
        static void PopulateProperties() { }

        static void VariableClicked(IOwnProgrammerInterface var)
        {
            SetInterface(var);
        }

        static void PropertyClicked(IOwnProgrammerInterface prop)
        {

        }

        static void SetInterface(IOwnProgrammerInterface owner)
        {
            CurrentOwner = owner;

            if (owner?.Interface is null)
                owner?.SetupInterface();

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
    }
}
