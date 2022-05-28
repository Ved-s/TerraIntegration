using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Reflection;
using TerraIntegration.Interfaces;
using Terraria.ID;
using Terraria.ModLoader;

namespace TerraIntegration.Values
{
    public class Tile : VariableValue, INamed
    {
        public override string Type => "tile";
        public override string TypeDisplay => "Tile";
        public override Color TypeColor => Microsoft.Xna.Framework.Color.SaddleBrown;

        public int TileType { get; set; }
        public int TileFrameX { get; set; }
        public int TileFrameY { get; set; }
        public int TileFrameNumber { get; set; }
        public byte Liquid { get; set; }
        public byte Color { get; set; }
        public byte Slope { get; set; }
        public bool HalfBlock { get; set; }
        public bool Actuated { get; set; }

        public bool Actuator { get; set; }
        public bool RedWire { get; set; }
        public bool GreenWire { get; set; }
        public bool BlueWire { get; set; }
        public bool YellowWire { get; set; }

        public string Name
        {
            get
            {
                if (TileType < TileID.Count)
                {
                    if (VanillaTileNameCache.TryGetValue(TileType, out string vanillaName))
                        return vanillaName;
                    return null;
                }
                return TileLoader.GetTile(TileType)?.Name;
            }
        }

        static Dictionary<int, string> VanillaTileNameCache = new();

        static Tile()
        {
            foreach (FieldInfo field in typeof(TileID).GetFields())
            {
                if (field.IsLiteral && field.FieldType == typeof(ushort) && field.GetRawConstantValue() is ushort id)
                {
                    VanillaTileNameCache[id] = field.Name;
                }
            }
        }

        public Tile() { TileType = -1; }
        public Tile(Terraria.Tile tile)
        {
            TileType = tile.TileType;
            TileFrameX = tile.TileFrameX;
            TileFrameY = tile.TileFrameY;
            TileFrameNumber = tile.TileFrameNumber;
            Liquid = tile.LiquidAmount;
            Color = tile.TileColor;
            Slope = (byte)tile.Slope;
            HalfBlock = tile.IsHalfBlock;
            Actuated = tile.IsActuated;
            Actuator = tile.HasActuator;
            RedWire = tile.RedWire;
            GreenWire = tile.GreenWire;
            BlueWire = tile.BlueWire;
            YellowWire = tile.YellowWire;
        }
        public Tile(int type)
        {
            TileType = type;
        }
    }

    public class Wall : VariableValue, INamed
    {
        public override string Type => "wall";
        public override string TypeDisplay => "Wall";
        public override Color TypeColor => Microsoft.Xna.Framework.Color.Brown;

        public int WallType { get; set; }
        public int WallFrameX { get; set; }
        public int WallFrameY { get; set; }
        public int WallFrameNumber { get; set; }
        public byte Color { get; set; }

        public string Name
        {
            get
            {
                if (WallType < WallID.Count)
                {
                    if (VanillaWallNameCache.TryGetValue(WallType, out string vanillaName))
                        return vanillaName;
                    return null;
                }
                return WallLoader.GetWall(WallType)?.Name;
            }
        }

        static Dictionary<int, string> VanillaWallNameCache = new();

        static Wall()
        {
            foreach (FieldInfo field in typeof(WallID).GetFields())
            {
                if (field.IsLiteral && field.FieldType == typeof(ushort) && field.GetRawConstantValue() is ushort id)
                {
                    VanillaWallNameCache[id] = field.Name;
                }
            }
        }

        public Wall() { }
        public Wall(Terraria.Tile tile)
        {
            WallType = tile.WallType;
            WallFrameX = tile.WallFrameX;
            WallFrameY = tile.WallFrameY;
            WallFrameNumber = tile.WallFrameNumber;
            Color = tile.WallColor;
        }
        public Wall(int type)
        {
            WallType = type;
        }
    }
}
