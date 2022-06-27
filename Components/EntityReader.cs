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
    public class EntityReader : Component
    {
        public override string TypeName => "entity";
        public override string TypeDefaultDisplayName => "Entity reader";
        public override string TypeDefaultDescription => "Reads information entities in front of it.";

        //public override SpriteSheet DefaultPropertySpriteSheet { get; set; } = new("TerraIntegration/Assets/Types/entity", new(32, 32));

        public override void SetStaticDefaults()
        {
            SetupNewTile();
            Main.tileFrameImportant[Type] = true;

            TileObjectData.addTile(Type);

            ItemDrop = ModContent.ItemType<Items.ComponentItems.EntityReader>();
        }
    }
}
