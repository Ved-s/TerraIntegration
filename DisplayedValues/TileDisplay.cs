using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;

namespace TerraIntegration.DisplayedValues
{
    public class TileDisplay : DisplayedValue
    {
        public override string Type => "tile";
        public override string HoverText { get; }

        public ushort TileType;
        public Point16 TileFrame;
        public byte TileColor;
        public float MaxZoom = 1f;

        public TileDisplay() { }
        public TileDisplay(ushort type, Point16 frame, byte color, string hoverText, float maxZoom = 1f) 
        {
            TileType = type;
            TileFrame = frame;
            TileColor = color;
            HoverText = hoverText;
            MaxZoom = maxZoom;
        }

        public override void Draw(Rectangle screenRect, SpriteBatch spriteBatch)
        {
            Texture2D texture = Main.instance.TilePaintSystem.TryGetTileAndRequestIfNotReady(TileType, 0, TileColor);
            if (texture is null) return;

            Tile temp = Main.tile[1, 1];

            temp.TileType = TileType;
            temp.TileColor = TileColor;
            short tileFrameX = temp.TileFrameX = TileFrame.X;
            short tileFrameY = temp.TileFrameY = TileFrame.Y;

            Main.instance.TilesRenderer.GetTileDrawData(1, 1, temp, TileType, 
                ref tileFrameX, ref tileFrameY,
                out int tileWidth, out int tileHeight,
                out _, out _, out int addFrX, out int addFrY,
                out SpriteEffects effects, out _, out _, out _);

            temp.ClearTile();

            tileFrameX += (short)addFrX;
            tileFrameY += (short)addFrY;

            float zoomH = screenRect.Width / (float)tileWidth;
            float zoomV = screenRect.Height / (float)tileHeight;
            float minZoom = Math.Min(zoomH, zoomV);
            float zoom = Math.Min(MaxZoom, minZoom);

            Vector2 pos = screenRect.Location.ToVector2();
            pos += (screenRect.Size() - new Vector2(tileWidth, tileHeight) * zoom) / 2;

            Rectangle frame = new(tileFrameX, tileFrameY, tileWidth, tileHeight);

            spriteBatch.Draw(texture, pos, frame, Color.White, 0f, Vector2.Zero, zoom, effects, 0f);
        }

        protected override DisplayedValue ReceiveCustomData(BinaryReader reader)
        {
            return new TileDisplay(reader.ReadUInt16(), new(reader.ReadInt16(), reader.ReadInt16()), reader.ReadByte(), reader.ReadString(), reader.ReadSingle());
        }

        protected override void SendCustomData(BinaryWriter writer)
        {
            writer.Write(TileType);
            writer.Write(TileFrame.X);
            writer.Write(TileFrame.Y);
            writer.Write(TileColor);
            writer.Write(HoverText);
            writer.Write(MaxZoom);
        }

        public override bool Equals(DisplayedValue value)
        {
            return value is TileDisplay display &&
                   HoverText == display.HoverText &&
                   TileType == display.TileType &&
                   TileFrame == display.TileFrame &&
                   TileColor == display.TileColor &&
                   MaxZoom == display.MaxZoom;
        }
    }
}
