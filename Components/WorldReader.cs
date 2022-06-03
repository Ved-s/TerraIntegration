using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        public override string ComponentType => "world";
        public override string ComponentDisplayName => "World reader";

        public override SpriteSheet DefaultPropertySpriteSheet { get; set; } = new("TerraIntegration/Assets/Types/world", new(32, 32));

        public override Vector2 InterfaceOffset => new(24, 0);

        public override void SetStaticDefaults()
        {
            Main.tileFrameImportant[Type] = true;

            SetupNewTile();
            TileObjectData.addTile(Type);

            ItemDrop = ModContent.ItemType<Items.ComponentItems.WorldReader>();
        }
    }
}
