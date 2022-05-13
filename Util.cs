using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;

namespace TerraIntegration
{
    public static class Util
    {
        public static void DropItemInWorld(Item item, int x, int y)
        {
            Main.item[400] = new Item();
            int num = 400;
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                num = PickAnItemSlotToSpawnItemOn(false, num);
            }

            Main.timeItemSlotCannotBeReusedFor[num] = 0;
            Main.item[num] = item;
            item.position.X = x - item.width / 2;
            item.position.Y = y - item.height / 2;
            item.wet = Collision.WetCollision(item.position, item.width, item.height);
            item.velocity.X = Main.rand.Next(-30, 31) * 0.1f;
            item.velocity.Y = Main.rand.Next(-40, -15) * 0.1f;

			if (Main.netMode == NetmodeID.Server)
			{
				NetMessage.SendData(MessageID.SyncItem, -1, -1, null, num, 1);
			}
			else if (Main.netMode == NetmodeID.SinglePlayer)
			{
				item.playerIndexTheItemIsReservedFor = Main.myPlayer;
			}
		}

		public static int PickAnItemSlotToSpawnItemOn(bool reverseLookup, int nextItem)
		{
			int num = 0;
			int num2 = 400;
			int num3 = 1;
			if (reverseLookup)
			{
				num = 399;
				num2 = -1;
				num3 = -1;
			}
			bool flag = false;
			for (int i = num; i != num2; i += num3)
			{
				if (!Main.item[i].active && Main.timeItemSlotCannotBeReusedFor[i] == 0)
				{
					nextItem = i;
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				int num4 = 0;
				for (int j = 0; j < 400; j++)
				{
					if (Main.timeItemSlotCannotBeReusedFor[j] == 0 && !Main.item[j].instanced && Main.item[j].timeSinceItemSpawned > num4)
					{
						num4 = Main.item[j].timeSinceItemSpawned;
						nextItem = j;
						flag = true;
					}
				}
				if (!flag)
				{
					for (int k = 0; k < 400; k++)
					{
						if (Main.item[k].timeSinceItemSpawned - Main.timeItemSlotCannotBeReusedFor[k] > num4)
						{
							num4 = Main.item[k].timeSinceItemSpawned - Main.timeItemSlotCannotBeReusedFor[k];
							nextItem = k;
						}
					}
				}
			}
			return nextItem;
		}

		public static string ColorTag(Color color, string text) 
		{
			if (text is null)
				return null;

			uint v = color.PackedValue; // AABBGGRR

			v = (v & 0xff0000) >> 16 | (v & 0x00ff00) | (v & 0x0000ff) << 16; // RRGGBB

			return $"[c/{v:x6}:{text.Replace("\n", $"]\n[c/{v:x6}:")}]";

		}


		public static bool ObjectsNullEqual(object a, object b)
		{
			if (a is not null && b is not null) 
				return a.Equals(b);
			return a is null && b is null;
		}
	}
}
