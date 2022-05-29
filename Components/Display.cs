using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using TerraIntegration.DisplayedValues;
using TerraIntegration.UI;
using TerraIntegration.Values;
using TerraIntegration.Variables;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace TerraIntegration.Components
{
    public class DisplayData : ComponentData
    {
        public bool FrameScanCompleted = false;
        public Point16 MasterPos = default;
        public Point16 DisplaySize = default;

        public MasterDisplayData Master;

        public void NoMoreMaster(Point16 pos)
        {
            Variable v = GetVariable(Display.DisplayVariableSlot);
            if (v is not null)
            {
                DisplayData master = (Component as Display).GetData(MasterPos);
                Variable masterVar = master.GetVariable(Display.DisplayVariableSlot);
                if (masterVar is null)
                {
                    master.SetVariable(Display.DisplayVariableSlot, v);
                    ClearVariable(Display.DisplayVariableSlot);
                    master.Component.OnVariableChanged(MasterPos, Display.DisplayVariableSlot);
                    Component.OnVariableChanged(pos, Display.DisplayVariableSlot);
                    return;
                }

                Util.DropItemInWorld(GetVariableItem(Display.DisplayVariableSlot).Item, pos.X * 16, pos.Y * 16);
                ClearVariable(Display.DisplayVariableSlot);
                Component.OnVariableChanged(pos, Display.DisplayVariableSlot);
            }
        }
    }

    public class MasterDisplayData
    {
        public DisplayedValue DisplayValue;

        public string SentErrors;
        public DisplayedValue SentDisplayValue;
    }

    public class Display : Component<DisplayData>
    {
        public const string DisplayVariableSlot = "display";

        public readonly static SpriteSheet TypeSheet = new("TerraIntegration/Assets/Types/display", new(32, 32));

        public override string ComponentType => "display";
        public override string ComponentDisplayName => "Variable display";

        public override bool HasCustomInterface => true;

        public override ushort DefaultUpdateFrequency => 15;

        public override SpriteSheet DefaultPropertySpriteSheet => TypeSheet;

        public override bool CanHaveVariables => true;

        private UIComponentVariable Slot = new();
        private List<Error> Errors = new();

        public override void SetStaticDefaults()
        {
            Main.tileSolid[Type] = false;
            SetupNewTile();
            TileObjectData.addTile(Type);

            ItemDrop = ModContent.ItemType<Items.ComponentItems.Display>();
        }
        public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak)
        {
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                Point16 pos = new(i, j);
                if (!GetData(pos).FrameScanCompleted)
                    ScanAndUpdateDisplayFrames(pos);
            }
            return false;
        }
        public override void PostDraw(int i, int j, SpriteBatch spriteBatch)
        {
            Point16 pos = new(i, j);
            DisplayData data = GetData(pos);

            Point16 drawTile = data.MasterPos + data.DisplaySize - new Point16(1, 1);

            if (drawTile != pos) return;

            Rectangle screenRect = new();

            Vector2 screen = data.MasterPos.ToVector2() * 16 + new Vector2(Main.offScreenRange) - Main.screenPosition;
            screenRect.X = (int)screen.X;
            screenRect.Y = (int)screen.Y;
            screenRect.Width = 16 * data.DisplaySize.X;
            screenRect.Height = 16 * data.DisplaySize.Y;

            screenRect.X += 2;
            screenRect.Y += 2;
            screenRect.Width -= 4;
            screenRect.Height -= 4;

            data = GetData(data.MasterPos);

            DisplayDraw(data, screenRect, spriteBatch);
        }

        public override void OnPlaced(Point16 pos)
        {
            base.OnPlaced(pos);
            ScanAndUpdateDisplayFrames(pos);
        }
        public override void OnKilled(Point16 pos)
        {
            base.OnKilled(pos);
            DisplayData data = GetData(pos);
            if (data.Master is not null)
                data.NoMoreMaster(pos);
            ScanAndUpdateDisplayFrames(pos, true);
        }
        public override void OnUpdate(Point16 pos)
        {
            base.OnUpdate(pos);
            DisplayData data = GetData(pos);

            if (data.Master is null) return;
            if (!data.HasVariable(DisplayVariableSlot))
            {
                data.Master.DisplayValue = null;
                SyncNull(pos, data.Master);
                return;
            }
            Errors.Clear();

            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                Values.VariableValue value = data.GetVariable(DisplayVariableSlot).GetValue(data.System, Errors);
                if (Errors.Count > 0)
                {
                    data.Master.DisplayValue = new ErrorDisplay(Errors.ToArray());
                    SyncValue(pos, data.Master);
                    return;
                }
                if (value is null)
                {
                    data.Master.DisplayValue = null;
                    SyncNull(pos, data.Master);
                    return;
                }
                data.Master.DisplayValue = value.Display(data.System);
                SyncValue(pos, data.Master);
            }
        }
        public override void OnSystemUpdate(Point16 pos)
        {
            DisplayData data = GetData(pos);

            if (!data.FrameScanCompleted)
                ScanAndUpdateDisplayFrames(pos);
        }
        public override void OnPlayerJoined(int player)
        {
            base.OnPlayerJoined(player);

            List<DisplayBoundaryData> boundaries = new();
            List<(Point16, MasterDisplayData)> displays = new();

            foreach (KeyValuePair<Point16, ComponentData> kvp in World.ComponentData)
                if (kvp.Value is DisplayData display && display.Master is not null)
                {
                    boundaries.Add(new(display.MasterPos, display.DisplaySize));
                    displays.Add((kvp.Key, display.Master));
                }

            SendBoundaries(boundaries);
            foreach (var (pos, data) in displays)
            {
                SyncValue(pos, data, false);
            }
        }

        private void SendBoundaries(List<DisplayBoundaryData> boundaries)
        {
            ModPacket pack = CreatePacket(default, (ushort)MessageType.DisplayBoundaries);
            pack.Write((ushort)boundaries.Count);
            for (int i = 0; i < boundaries.Count; i++)
            {
                DisplayBoundaryData boundary = boundaries[i];
                pack.Write(boundary.Master.X);
                pack.Write(boundary.Master.Y);
                pack.Write(boundary.Size.X);
                pack.Write(boundary.Size.Y);
            }
            pack.Send();
        }

        public override bool HandlePacket(Point16 pos, ushort messageType, BinaryReader reader, int whoAmI, ref bool broadcast)
        {
            DisplayData data = GetData(pos);

            switch ((MessageType)messageType)
            {
                case MessageType.DisplayBoundaries:
                    ushort count = reader.ReadUInt16();
                    for (int i = 0; i < count; i++)
                    {
                        Point16 master = new(reader.ReadInt16(), reader.ReadInt16());
                        Point16 size = new(reader.ReadInt16(), reader.ReadInt16());
                        SetDisplayRectangle(null, size, master);
                    }
                    return true;

                case MessageType.DisplayNull:
                    if (data.Master is null) break;
                    data.Master.DisplayValue = null;
                    return true;

                case MessageType.DisplayValue:
                    if (data.Master is null) break;
                    data.Master.DisplayValue = DisplayedValue.ReceiveData(reader);
                    return true;
            }
            return false;
        }

        public void SyncNull(Point16 pos, MasterDisplayData data, bool changedOnly = true)
        {
            if (Main.netMode == NetmodeID.SinglePlayer) return;

            if (changedOnly)
            {
                if (data.SentErrors is null &&
                    data.SentDisplayValue is null)
                    return;
            }

            CreatePacket(pos, (ushort)MessageType.DisplayNull).Send();
            data.SentErrors = null;
            data.SentDisplayValue = null;
        }
        public void SyncValue(Point16 pos, MasterDisplayData data, bool changedOnly = true)
        {
            if (Main.netMode == NetmodeID.SinglePlayer) return;

            if (changedOnly)
                if (data.DisplayValue.Equals(data.SentDisplayValue))
                    return;

            ModPacket p = CreatePacket(pos, (ushort)MessageType.DisplayValue);

            if (data.DisplayValue is null)
                p.Write("");
            else
                data.DisplayValue.SendData(p);
            p.Send();
            data.SentDisplayValue = data.DisplayValue;
        }

        public override string GetHoverText(Point16 pos)
        {
            DisplayData data = GetData(pos);

            if (data.MasterPos == default)
                return null;

            data = GetData(data.MasterPos);

            if (data.Master?.DisplayValue is null)
                return null;


            return data.Master.DisplayValue.HoverText;
        }
        public override bool ShouldSaveData(DisplayData data) => data.Master is not null;
        public override UIPanel SetupInterface()
        {
            UIPanel p = new()
            {
                Width = new(0, 1),
                Height = new(58, 0),
                PaddingTop = 0,
                PaddingLeft = 0,
                PaddingRight = 0,
                PaddingBottom = 0,
            };
            Slot = new()
            {
                Top = new(8, 0),
                Left = new(-21, 0.5f),
                VariableSlot = DisplayVariableSlot,
            };
            p.Append(Slot);

            return p;
        }
        public override Point16 GetInterfaceTarget(Point16 pos)
        {
            DisplayData data = GetData(pos);
            return new(data.MasterPos.X, data.MasterPos.Y);
        }
        public override void UpdateInterface(Point16 pos)
        {
            DisplayData data = GetData(pos);
            Vector2 off = new(data.DisplaySize.X * 16, 0);

            off.X += 8;

            InterfaceOffset = off;

            Slot.Component = new(data.MasterPos, this);
        }
        public override bool CheckShowInterface(Point16 pos)
        {
            return GetData(pos).Master is not null;
        }
        public override Vector2 GetInterfaceReachCheckPos(Point16 pos)
        {
            DisplayData data = GetData(pos);

            Rectangle frame = new(
                data.MasterPos.X * 16,
                data.MasterPos.Y * 16,
                data.DisplaySize.X * 16,
                data.DisplaySize.Y * 16
                );

            Vector2 playerCenter = Main.LocalPlayer.Center;
            if (playerCenter.X < frame.X)
                playerCenter.X = frame.X;
            if (playerCenter.Y < frame.Y)
                playerCenter.Y = frame.Y;
            if (playerCenter.X > frame.Right)
                playerCenter.X = frame.Right;
            if (playerCenter.Y > frame.Bottom)
                playerCenter.Y = frame.Bottom;

            return playerCenter;
        }

        private void ScanAndUpdateDisplayFrames(Point16 pos, bool skipMe = false)
        {
            if (Main.netMode == NetmodeID.MultiplayerClient) return;

            HashSet<Point16> found = new();
            Queue<Point16> queue = new();
            queue.Enqueue(pos);

            int displayType = ModContent.TileType<Display>();

            while (queue.Count > 0)
            {
                Point16 p = queue.Dequeue();
                if (found.Contains(p))
                    continue;

                found.Add(p);

                Point16 check = new(p.X, p.Y - 1);
                if (Main.tile[check.X, check.Y].TileType == displayType && !found.Contains(check))
                    queue.Enqueue(check);

                check = new(p.X - 1, p.Y);
                if (Main.tile[check.X, check.Y].TileType == displayType && !found.Contains(check))
                    queue.Enqueue(check);

                check = new(p.X, p.Y + 1);
                if (Main.tile[check.X, check.Y].TileType == displayType && !found.Contains(check))
                    queue.Enqueue(check);

                check = new(p.X + 1, p.Y);
                if (Main.tile[check.X, check.Y].TileType == displayType && !found.Contains(check))
                    queue.Enqueue(check);
            }

            if (skipMe)
                found.Remove(pos);

            List<DisplayBoundaryData> boundaries = new();

            while (found.Count > 0)
            {
                Point16 maxSize = default;
                int maxArea = 0;
                Point16 maxAreaPoint = default;

                foreach (Point16 p in found)
                {
                    Point16 rect = GetDisplayRectangle(p, found);
                    int area = rect.X * rect.Y;

                    if (area > maxArea)
                    {
                        maxArea = area;
                        maxAreaPoint = p;
                        maxSize = rect;
                    }
                }
                boundaries.Add(new(maxAreaPoint, maxSize));
                SetDisplayRectangle(found, maxSize, maxAreaPoint);
            }
            if (Main.netMode == NetmodeID.Server)
                SendBoundaries(boundaries);
        }

        private void SetDisplayRectangle(HashSet<Point16> setToRemoveFrom, Point16 size, Point16 master)
        {
            for (int dy = 0; dy < size.Y; dy++)
                for (int dx = 0; dx < size.X; dx++)
                {
                    Point16 p = new(master.X + dx, master.Y + dy);

                    short frameX = 0, frameY = 0;

                    if (dy > 0) frameY += 18;
                    if (dx > 0) frameX += 18;
                    if (dy < size.Y - 1) frameY += 36;
                    if (dx < size.X - 1) frameX += 36;

                    Terraria.Tile t = Main.tile[p.X, p.Y];
                    t.TileFrameX = frameX;
                    t.TileFrameY = frameY;

                    DisplayData data = GetData(p);
                    data.MasterPos = master;
                    data.DisplaySize = size;
                    data.FrameScanCompleted = true;

                    bool isMaster = dx == 0 && dy == 0;

                    SetUpdates(p, isMaster);

                    if (!isMaster && data.Master is not null)
                    {
                        data.NoMoreMaster(p);
                        data.Master = null;
                    }
                    else if (isMaster && data.Master is null)
                    {
                        data.Master = new();
                    }

                    setToRemoveFrom?.Remove(p);
                }
        }

        static Point16 GetDisplayRectangle(Point16 p, HashSet<Point16> displays)
        {
            short width = 1;
            short height = 1;

            bool widthFail = false;
            bool heightFail = false;

            while (!widthFail || !heightFail)
            {
                Point16 check = p;
                check.X += width;

                widthFail = false;
                for (int y = 0; y < height; y++)
                {
                    if (!displays.Contains(check))
                    {
                        widthFail = true;
                        break;
                    }
                    check.Y++;
                }
                if (!widthFail)
                    width++;

                check = p;
                check.Y += height;

                heightFail = false;
                for (int x = 0; x < width; x++)
                {
                    if (!displays.Contains(check))
                    {
                        heightFail = true;
                        break;
                    }
                    check.X++;
                }
                if (!heightFail)
                    height++;
            }

            return new(width, height);
        }

        public void DisplayDraw(DisplayData data, Rectangle screenRect, SpriteBatch spriteBatch)
        {
            if (data.Master is null || data.Master.DisplayValue is null)
                return;

           data.Master.DisplayValue.Draw(screenRect, spriteBatch);
        }

        internal record struct DisplayBoundaryData(Point16 Master, Point16 Size);
        enum MessageType : ushort
        {
            DisplayBoundaries,
            DisplayNull,
            DisplayValue
        }
    }
}
