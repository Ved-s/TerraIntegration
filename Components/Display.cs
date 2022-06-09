using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Graphics;
using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using TerraIntegration.Basic;
using TerraIntegration.DataStructures;
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

        public DisplayedValue DisplayValue;
        public DisplayedValue SentDisplayValue;

        public void NoMoreMaster(Point16? newMasterPos)
        {
            Variable v = GetVariable(Display.DisplayVariableSlot);
            if (v is not null)
            {
                if (newMasterPos.HasValue)
                {
                    DisplayData master = (Component as Display).GetData(newMasterPos.Value);
                    Variable masterVar = master.GetVariable(Display.DisplayVariableSlot);
                    if (masterVar is null)
                    {
                        master.SetVariable(Display.DisplayVariableSlot, v);
                        ClearVariable(Display.DisplayVariableSlot);
                        master.Component.OnVariableChanged(newMasterPos.Value, Display.DisplayVariableSlot);
                        Component.OnVariableChanged(Position, Display.DisplayVariableSlot);
                        return;
                    }
                }

                Util.DropItemInWorld(GetVariableItem(Display.DisplayVariableSlot).Item, Position.X * 16, Position.Y * 16);
                ClearVariable(Display.DisplayVariableSlot);
                Component.OnVariableChanged(Position, Display.DisplayVariableSlot);
            }
        }
    }

    public class Display : Component<DisplayData>
    {
        public const string DisplayVariableSlot = "display";

        public readonly static SpriteSheet TypeSheet = new("TerraIntegration/Assets/Types/display", new(32, 32));

        public override string TypeName => "display";
        public override string TypeDefaultDisplayName => "Variable display";
        public override string TypeDefaultDescription => "Displays display variables.\nAlso they can be connected into bigger\ndisplays of rectangular shape.";

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
            if (Main.netMode != NetmodeID.MultiplayerClient && !TileMimicking.IsMimicking(i, j))
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

            Point16 drawTile = data.Position + data.Size - new Point16(1, 1);

            if (drawTile != pos) return;

            Rectangle screenRect = new();

            Vector2 screen = data.Position.ToVector2() * 16 + new Vector2(Main.offScreenRange) - Main.screenPosition;
            screenRect.X = (int)screen.X;
            screenRect.Y = (int)screen.Y;
            screenRect.Width = 16 * data.Size.X;
            screenRect.Height = 16 * data.Size.Y;

            screenRect.X += 2;
            screenRect.Y += 2;
            screenRect.Width -= 4;
            screenRect.Height -= 4;

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
            data.NoMoreMaster(null);
            ScanAndUpdateDisplayFrames(pos, true);
        }
        public override void OnUpdate(Point16 pos)
        {
            base.OnUpdate(pos);
            DisplayData data = GetData(pos);

            if (!data.HasVariable(DisplayVariableSlot))
            {
                data.DisplayValue = null;
                SyncNull(data);
                return;
            }
            Errors.Clear();

            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                Variable var = data.GetVariable(DisplayVariableSlot);
                VariableValue value = var?.GetValue(data.System, Errors);
                var?.SetLastValue(value, data.System);

                if (Errors.Count > 0)
                {
                    data.DisplayValue = new ErrorDisplay(Errors.ToArray());
                    SyncValue(data);
                    return;
                }
                if (value is null)
                {
                    data.DisplayValue = null;
                    SyncNull(data);
                    return;
                }
                data.DisplayValue = value.Display(data.System);
                SyncValue(data);
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
            List<DisplayData> displays = new();

            foreach (ComponentData data in World.EnumerateAllComponentData())
                if (data is DisplayData display)
                {
                    boundaries.Add(new(display.Position, display.Size));
                    displays.Add(display);
                }

            SendBoundaries(boundaries);
            foreach (DisplayData data in displays)
            {
                SyncValue(data, false);
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
                    data.DisplayValue = null;
                    return true;

                case MessageType.DisplayValue:
                    data.DisplayValue = DisplayedValue.ReceiveData(reader);
                    return true;
            }
            return false;
        }

        public void SyncNull(DisplayData data, bool changedOnly = true)
        {
            if (Main.netMode == NetmodeID.SinglePlayer) return;

            if (changedOnly)
            {
                if (data.SentDisplayValue is null)
                    return;
            }

            CreatePacket(data.Position, (ushort)MessageType.DisplayNull).Send();
            data.SentDisplayValue = null;
        }
        public void SyncValue(DisplayData data, bool changedOnly = true)
        {
            if (Main.netMode == NetmodeID.SinglePlayer) return;

            if (changedOnly)
                if (data.DisplayValue.Equals(data.SentDisplayValue))
                    return;

            ModPacket p = CreatePacket(data.Position, (ushort)MessageType.DisplayValue);

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

            if (data.DisplayValue is null)
                return null;


            return data.DisplayValue.HoverText;
        }
        public override bool ShouldSaveData(DisplayData data) => true;
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
            return data.Position;
        }
        public override void UpdateInterface(Point16 pos)
        {
            DisplayData data = GetData(pos);
            Vector2 off = new(data.Size.X * 16, 0);

            off.X += 8;

            InterfaceOffset = off;

            Slot.Component = new(data.Position, this);
        }
        public override Vector2 GetInterfaceReachCheckPos(Point16 pos)
        {
            DisplayData data = GetData(pos);

            Rectangle frame = new(
                data.Position.X * 16,
                data.Position.Y * 16,
                data.Size.X * 16,
                data.Size.Y * 16
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

            Point16 min = new(short.MaxValue, short.MaxValue);
            Point16 max = new(0, 0);

            while (queue.Count > 0)
            {
                Point16 p = queue.Dequeue();
                if (found.Contains(p))
                    continue;

                found.Add(p);

                min = new(Math.Min(min.X, p.X), Math.Min(min.Y, p.Y));
                max = new(Math.Max(max.X, p.X), Math.Max(max.Y, p.Y));

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

            int regionWidth = max.X - min.X;
            int regionHeight = max.Y - min.Y;

            while (found.Count > 0)
            {
                Point16 maxSize = default;
                int maxArea = 0;
                Point16 maxAreaPoint = default;

                foreach (Point16 p in found.OrderBy(p => 
                {
                    int relx = p.X - min.X;
                    int rely = p.Y - min.Y;

                    return relx + rely * regionWidth;
                }))
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

                    DisplayData data = GetDataOrNull(p);
                    bool isMaster = dx == 0 && dy == 0;

                    if (data is not null && !isMaster && data.Position != p)
                    {
                        data.NoMoreMaster(master);
                    }
                    else SetUpdates(p, true);

                    setToRemoveFrom?.Remove(p);
                }
            World.DefineMultitile(new(master.X, master.Y, size.X, size.Y));
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
            if (data.DisplayValue is null)
                return;

           data.DisplayValue.Draw(screenRect, spriteBatch);
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
