using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using TerraIntegration.UI;
using TerraIntegration.Variables;
using TerraIntegration.Variables.Display;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.GameInput;
using Terraria.ModLoader;
using Terraria.ObjectData;
using Terraria.UI;

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
            if (Variables[0] is not null)
            {
                DisplayData master = (Component as Display).GetData(MasterPos);
                if (master.Variables[0] is null)
                {
                    master.Variables[0] = Variables[0];
                    Variables[0] = null;
                    master.Component.OnVariableChanged(MasterPos, 0);
                    return;
                }

                Util.DropItemInWorld(Variables[0].Item, pos.X * 16, pos.Y * 16);
                Variables[0] = null;
            }
        }
    }

    public class MasterDisplayData 
    {
        public string DisplayTextCache = null;
        public Color DisplayColorCache = default;
    }

    public class Display : Component<DisplayData>
    {
        public override string ComponentType => "display";
        public override bool HasRightClickInterface => true;

        public override int VariableSlots => 1;

        private UIComponentVariable Slot = new();

        public override void SetStaticDefaults()
        {
            Main.tileSolid[Type] = false;
            SetupNewTile();
            TileObjectData.addTile(Type);

            ItemDrop = ModContent.ItemType<Items.Display>();
        }
        public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak)
        {
            Point16 pos = new(i, j);
            if (!GetData(pos).FrameScanCompleted)
                ScanAndUpdateDisplayFrames(pos);
            return false;
        }
        public override void PostDraw(int i, int j, SpriteBatch spriteBatch)
        {
            Point16 pos = new(i, j);
            DisplayData data = GetData(pos);

            Point16 drawTile = data.MasterPos + data.DisplaySize - new Point16(1, 1);

            if (drawTile != pos) return;

            Rectangle screenRect = new();

            Vector2 screen = (new Vector2(data.MasterPos.X + 12, data.MasterPos.Y + 12) * 16) - Main.screenPosition;
            screenRect.X = (int)screen.X;
            screenRect.Y = (int)screen.Y;
            screenRect.Width = 16 * data.DisplaySize.X;
            screenRect.Height = 16 * data.DisplaySize.Y;

            screenRect.X += 2;
            screenRect.Y += 2;
            screenRect.Width -= 4;
            screenRect.Height -= 4;

            data = GetData(data.MasterPos);

            DisplayDraw(pos, data, screenRect, spriteBatch);
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
            if (data.Variables[0] is null)
            {
                data.Master.DisplayTextCache = null;
                return;
            }

            HashSet<Error> errors = new();

            Values.VariableValue value = data.Variables[0].Var.GetValue(data.System, errors);

            if (errors.Count > 0)
            {
                data.Master.DisplayTextCache = $"Errors:\n{string.Join('\n', errors)}";
                data.Master.DisplayColorCache = Color.OrangeRed;
                return;
            }

            data.Master.DisplayTextCache = value.Display();
            data.Master.DisplayColorCache = value.DisplayColor;
        }
        public override void OnSystemUpdate(Point16 pos)
        {
            DisplayData data = GetData(pos);

            if (!data.FrameScanCompleted)
                ScanAndUpdateDisplayFrames(pos);
        }

        public override string GetHoverText(Point16 pos)
        {
            DisplayData data = GetData(pos);

            if (data.MasterPos == default) 
                return null;

            data = GetData(data.MasterPos);

            if (data.Master.DisplayTextCache is null) 
                return null;

            if (Main.keyState.PressingShift())
                return Util.ColorTag(data.Master.DisplayColorCache, data.Master.DisplayTextCache);

            Vector2 textSize = FontAssets.MouseText.Value.MeasureString(data.Master.DisplayTextCache);
            Vector2 displaySize = data.DisplaySize.ToVector2() * 16 - new Vector2(4);

            if (textSize.X > displaySize.X || textSize.Y > displaySize.Y)
                return Util.ColorTag(data.Master.DisplayColorCache, data.Master.DisplayTextCache);

            return null;
        }
        public override bool ShouldSaveData(DisplayData data) => data.Master is not null;
        public override UIPanel SetupInterface()
        {
            UIPanel p = new()
            {
                Width = new(56, 0),
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
                VariableSlot = 0,
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

        void ScanAndUpdateDisplayFrames(Point16 pos, bool skipMe = false)
        {
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

                for (int dy = 0; dy < maxSize.Y; dy++)
                    for (int dx = 0; dx < maxSize.X; dx++)
                    {
                        Point16 p = new(maxAreaPoint.X + dx, maxAreaPoint.Y + dy);

                        short frameX = 0, frameY = 0;

                        if (dy > 0) frameY += 18;
                        if (dx > 0) frameX += 18;
                        if (dy < maxSize.Y - 1) frameY += 36;
                        if (dx < maxSize.X - 1) frameX += 36;

                        Tile t = Main.tile[p.X, p.Y];
                        t.TileFrameX = frameX;
                        t.TileFrameY = frameY;

                        DisplayData data = GetData(p);
                        data.MasterPos = maxAreaPoint;
                        data.DisplaySize = maxSize;
                        data.FrameScanCompleted = true;

                        bool master = dx == 0 && dy == 0;

                        SetUpdates(p, master);

                        if (!master && data.Master is not null)
                        {
                            data.NoMoreMaster(p);
                            data.Master = null;
                        }
                        else if (master && data.Master is null)
                        {
                            data.Master = new();
                        }

                        found.Remove(p);
                    }
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
        
        public void DisplayDraw(Point16 pos, DisplayData data, Rectangle screenRect, SpriteBatch spriteBatch)
        {
            if (data.Master.DisplayTextCache is null) return;

            DrawTextCentered(spriteBatch, data.Master.DisplayTextCache, screenRect, data.Master.DisplayColorCache);
        }
        public void DrawTextCentered(SpriteBatch batch, string text, Rectangle rect, Color color)
        {
            Vector2 size = FontAssets.MouseText.Value.MeasureString(text);

            size.Y *= 0.8f;

            float zoomH = rect.Width / size.X;
            float zoomV = rect.Height / size.Y;

            float zoom = Math.Min(zoomH, zoomV);

            if (zoom < 1) size *= zoom;
            else zoom = 1f;

            Vector2 pos = rect.Location.ToVector2() + (rect.Size() / 2 - size / 2);

            batch.DrawString(FontAssets.MouseText.Value, text, pos, color, 0f, Vector2.Zero, zoom, SpriteEffects.None, 0);
        }
    }
}
