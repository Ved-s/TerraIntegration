using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Reflection;
using TerraIntegration.Basic;
using TerraIntegration.DataStructures;
using TerraIntegration.DisplayedValues;
using TerraIntegration.Interfaces;
using Terraria.ID;
using Terraria.ModLoader;

namespace TerraIntegration.Values
{
    public class Tile : VariableValue, INamed, ITyped
    {
        public override string Type => "tile";
        public override string TypeDisplay => "Tile";
        public override Color TypeColor => Microsoft.Xna.Framework.Color.SaddleBrown;

        public override SpriteSheetPos SpriteSheetPos => new(BasicSheet, 2, 2);

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
        int ITyped.Type => TileType;

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

        public override DisplayedValue Display(ComponentSystem system)
        {
            if (TileType < 0) return null;
            return new TileDisplay((ushort)TileType, new(TileFrameX, TileFrameY), Color, Name);
        }

        public override bool Equals(VariableValue obj)
        {
            return obj is Tile tile &&
                   TileType == tile.TileType &&
                   TileFrameX == tile.TileFrameX &&
                   TileFrameY == tile.TileFrameY &&
                   TileFrameNumber == tile.TileFrameNumber &&
                   Liquid == tile.Liquid &&
                   Color == tile.Color &&
                   Slope == tile.Slope &&
                   HalfBlock == tile.HalfBlock &&
                   Actuated == tile.Actuated &&
                   Actuator == tile.Actuator &&
                   RedWire == tile.RedWire &&
                   GreenWire == tile.GreenWire &&
                   BlueWire == tile.BlueWire &&
                   YellowWire == tile.YellowWire;
        }
    }
}
