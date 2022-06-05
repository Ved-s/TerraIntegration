using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Reflection;
using TerraIntegration.Basic;
using TerraIntegration.DataStructures;
using TerraIntegration.Interfaces;
using Terraria.ID;
using Terraria.ModLoader;

namespace TerraIntegration.Values
{
    public class Wall : VariableValue, INamed, ITyped
    {
        public override string Type => "wall";
        public override string TypeDisplay => "Wall";
        public override Color TypeColor => Microsoft.Xna.Framework.Color.Brown;
        
        public override SpriteSheetPos SpriteSheetPos => new(BasicSheet, 3, 2);

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
        int ITyped.Type => WallType;

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

        public override bool Equals(VariableValue obj)
        {
            return obj is Wall wall &&
                   WallType == wall.WallType &&
                   WallFrameX == wall.WallFrameX &&
                   WallFrameY == wall.WallFrameY &&
                   WallFrameNumber == wall.WallFrameNumber &&
                   Color == wall.Color;
        }
    }
}
