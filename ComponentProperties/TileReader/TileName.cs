using System;
using System.Collections.Generic;
using System.Reflection;
using TerraIntegration.Values;
using TerraIntegration.Variables;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace TerraIntegration.ComponentProperties.TileReader
{
    public class TileName : ComponentProperty<Components.TileReader>
    {
        public override string PropertyName => "tileName";
        public override string PropertyDisplay => "Tile name";

        public override Type VariableReturnType => typeof(Values.String);

        static Dictionary<ushort, string> VanillaTileNameCache = new();

        static TileName() 
        {
            foreach (FieldInfo field in typeof(TileID).GetFields())
            {
                if (field.IsLiteral && field.FieldType == typeof(ushort) && field.GetRawConstantValue() is ushort id)
                {
                    VanillaTileNameCache[id] = field.Name;
                }
            }
        }

        public override VariableValue GetProperty(Components.TileReader component, Point16 pos, List<Error> errors)
        {
            Tile tile = Framing.GetTileSafely(component.GetTargetTile(pos));
            string name = "";

            if (tile.HasTile)
            {
                if (tile.TileType < TileID.Count)
                {
                    if (VanillaTileNameCache.TryGetValue(tile.TileType, out string vanillaName))
                        name = vanillaName;
                }
                else if (tile.TileType < TileLoader.TileCount)
                {
                    ModTile modTile = TileLoader.GetTile(tile.TileType);
                    name = modTile.Name;
                }
            }
            return new Values.String(name);
        }
    }
}
