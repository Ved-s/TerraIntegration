using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerraIntegration.Basic;
using TerraIntegration.DataStructures;
using Terraria;
using Terraria.Audio;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace TerraIntegration.Components
{
    public class WorldReader : Component
    {
        public override string TypeName => "world";
        public override string TypeDefaultDisplayName => "World reader";
        public override string TypeDefaultDescription => "Reads information about the world.";

        public override SpriteSheet DefaultPropertySpriteSheet { get; set; } = new("TerraIntegration/Assets/Types/world", new(32, 32));

        public override void SetStaticDefaults()
        {
            Main.tileFrameImportant[Type] = true;

            SetupNewTile();
            TileObjectData.addTile(Type);

            ItemDrop = ModContent.ItemType<Items.ComponentItems.WorldReader>();
        }
    }
}
