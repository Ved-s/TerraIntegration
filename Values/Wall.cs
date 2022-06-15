using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Reflection;
using TerraIntegration.Basic;
using TerraIntegration.DataStructures;
using TerraIntegration.Interfaces;
using TerraIntegration.UI;
using TerraIntegration.Variables;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.ModLoader;

namespace TerraIntegration.Values
{
    public class Wall : VariableValue, INamed, ITyped, IProgrammable
    {
        public override string TypeName => "wall";
        public override string TypeDefaultDisplayName => "Wall";
        public override string TypeDefaultDescription => "Copy of a wall";

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

        public UI.UIItemSlot WallSlot;
        public UIPanel Interface { get; set; }
        public bool HasComplexInterface => false;

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

        public void SetupInterface()
        {
            Interface.Append(WallSlot = new()
            {
                Top = new(-21, .5f),
                Left = new(-21, .5f),

                DisplayOnly = true,
                ItemValidator = (item) => item.createWall > WallID.None,
                HoverText = "Any item that creates a wall",
                MaxSlotCapacity = 1
            });
        }

        public Variable WriteVariable()
        {
            if (WallSlot?.Item?.createWall is null or <= WallID.None)
            {
                WallSlot?.NewFloatingText(TerraIntegration.Localize("ProgrammingErrors.NoItem"), Microsoft.Xna.Framework.Color.Red);
                return null;
            }

            return new Constant(new Wall(WallSlot.Item.createWall));
        }
    }
}
