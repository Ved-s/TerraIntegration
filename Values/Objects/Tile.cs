using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Reflection;
using TerraIntegration.Basic;
using TerraIntegration.DataStructures;
using TerraIntegration.DisplayedValues;
using TerraIntegration.Interfaces;
using TerraIntegration.Interfaces.Value;
using TerraIntegration.ValueProperties;
using TerraIntegration.Variables;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.ModLoader;

namespace TerraIntegration.Values.Objects
{
    public class Tile : VariableValue, INamed, ITyped, IProgrammable
    {
        public override string TypeName => "tile";
        public override string TypeDefaultDisplayName => "Tile";
        public override string TypeDefaultDescription => "Copy of a tile";

        public override Color TypeColor => Microsoft.Xna.Framework.Color.SaddleBrown;

        public override SpriteSheetPos SpriteSheetPos => new(ObjectSheet, 0, 0);

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

        public UI.UIItemSlot TileSlot;
        public UIPanel Interface { get; set; }
        public bool HasComplexInterface => false;

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

        public override void OnRegister()
        {
            AutoProperty<Tile, Boolean>.Register(new("actuated", "Is actuated", (sys, v, err) => v.Actuated)
            {
                PropertyDescription = "Returns whether this tile is actuated",
                SpriteSheetPos = new(Variable.TileSheet, 2, 1)
            });
            AutoProperty<Tile, Boolean>.Register(new("actuator", "Has actuator", (sys, v, err) => v.Actuator)
            {
                PropertyDescription = "Returns whether this tile has an actuator on it",
                SpriteSheetPos = new(Variable.TileSheet, 3, 1)
            });

            AutoProperty<Tile, Boolean>.Register(new("wireRed", "Has red wire", (sys, v, err) => v.RedWire)
            {
                PropertyDescription = "Returns whether this tile has red wire on it",
                SpriteSheetPos = new(Variable.TileSheet, 0, 2)
            });
            AutoProperty<Tile, Boolean>.Register(new("wireGreen", "Has green wire", (sys, v, err) => v.GreenWire)
            {
                PropertyDescription = "Returns whether this tile has green wire on it",
                SpriteSheetPos = new(Variable.TileSheet, 1, 2)
            });
            AutoProperty<Tile, Boolean>.Register(new("wireBlue", "Has blue wire", (sys, v, err) => v.BlueWire)
            {
                PropertyDescription = "Returns whether this tile has blue wire on it",
                SpriteSheetPos = new(Variable.TileSheet, 2, 2)
            });
            AutoProperty<Tile, Boolean>.Register(new("wireYellow", "Has yellow wire", (sys, v, err) => v.YellowWire)
            {
                PropertyDescription = "Returns whether this tile has yellow wire on it",
                SpriteSheetPos = new(Variable.TileSheet, 3, 2)
            });

            AutoProperty<Tile, Boolean>.Register(new("half", "Is half block", (sys, v, err) => v.HalfBlock)
            {
                PropertyDescription = "Returns whether this tile is a half block",
                SpriteSheetPos = new(Variable.TileSheet, 0, 3)
            });

            AutoProperty<Tile, Byte>.Register(new("liquid", "Liquid level", (sys, v, err) => v.Liquid)
            {
                PropertyDescription = "Returns liquid level in tile",
                SpriteSheetPos = new(Variable.TileSheet, 2, 3)
            });

            AutoProperty<Tile, Byte>.Register(new("slope", "Slope type", (sys, v, err) => v.Slope)
            {
                PropertyDescription = "Returns tile slope type",
                SpriteSheetPos = new(Variable.TileSheet, 1, 3)
            });
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

        public void SetupInterface()
        {
            Interface.Append(TileSlot = new()
            {
                Top = new(-21, .5f),
                Left = new(-21, .5f),

                DisplayOnly = true,
                ItemValidator = (item) => item.createTile >= TileID.Dirt,
                HoverText = "Any item that creates a tile",
                MaxSlotCapacity = 1
            });
        }

        public Variable WriteVariable()
        {
            if (TileSlot?.Item?.createTile is null or < 0)
            {
                TileSlot?.NewFloatingText(TerraIntegration.Localize("ProgrammingErrors.NoItem"), Microsoft.Xna.Framework.Color.Red);
                return null;
            }

            return new Constant(new Tile(TileSlot.Item.createTile));
        }
    }
}
