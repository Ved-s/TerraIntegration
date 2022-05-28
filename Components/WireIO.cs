using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerraIntegration.DataStructures;
using TerraIntegration.UI;
using TerraIntegration.Variables;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader;
using Terraria.ObjectData;
using Terraria.UI;

namespace TerraIntegration.Components
{
    public class WireIO : Component
    {
        public const string OnSignalVariableSlot = "onsignal";
        public const string DoSignalVariableSlot = "dosignal";

        public override string ComponentType => "wire";
        public override string ComponentDisplayName => "Wire IO";
        public override string ComponentDescription => "Wire IO allows to interact with\nTerraria wiring system";

        public override bool CanHaveVariables => true;

        public override Vector2 InterfaceOffset => new(24, 0);

        public WireIO() 
        {
            VariableInfo = new ComponentVariableInfo[]
            {
                new() 
                {
                    AcceptVariableTypes = new[] { "event" },
                    VariableName = "On Signal",
                    VariableSlot = OnSignalVariableSlot,
                    VariableDescription = "An event that will be triggered\nonce wire signal been received"
                },
                new()
                {
                    AcceptVariableTypes = new[] { "eventsub" },
                    VariableName = "Do Signal",
                    VariableSlot = DoSignalVariableSlot,
                    VariableDescription = "Wire signal will be sent on this triggered"
                }

            };
        }

        public override void SetStaticDefaults()
        {
            Main.tileSolid[Type] = false;
            Main.tileFrameImportant[Type] = true;
            SetupNewTile();
            TileObjectData.addTile(Type);

            ItemDrop = ModContent.ItemType<Items.ComponentItems.VarStore>();
        }

        public override void OnEvent(Point16 pos, string slot)
        {
            base.OnEvent(pos, slot);
            if (slot == DoSignalVariableSlot)
                World.WireTrips.Add(new(pos.X, pos.Y, 1, 1, 0));
        }

        public override void HitWire(int i, int j)
        {
            Point16 pos = new(i, j);

            ComponentData data = GetData(pos);
            if (data.GetVariable(OnSignalVariableSlot) is Event ev)
                ev.Trigger(pos, data.System);
        }
    }
}
