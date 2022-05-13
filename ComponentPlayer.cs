using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerraIntegration.Components;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace TerraIntegration
{
    public class ComponentPlayer : ModPlayer
    {
        public override void SyncPlayer(int toWho, int fromWho, bool newPlayer)
        {
            if (Main.netMode == NetmodeID.Server)
            {
                Networking.SendComponentDataSync(null, Player.whoAmI);
            }
            foreach (Component c in Component.ByTypeName.Values)
                c.OnPlayerJoined(Player.whoAmI);
        }
    }
}
