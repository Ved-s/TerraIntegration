using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace TerraIntegration
{
    public static class Util
    {
		static Regex NotTagText = new(@"(?<=^|\[[^\]]+\])([\s\S]*?)(?=$|\[[^\]]+\])", RegexOptions.Compiled);

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

			if (Main.netMode != NetmodeID.SinglePlayer)
			{
				NetMessage.SendData(MessageID.SyncItem, -1, -1, null, num, 1);
			}
			else
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

			if (text.Length == 0) return "";

			uint v = color.PackedValue; // AABBGGRR
			v = (v & 0xff0000) >> 16 | (v & 0x00ff00) | (v & 0x0000ff) << 16; // RRGGBB
			string tagStart = $"[c/{v:x6}:";

			return NotTagText.Replace(text, m =>
			{
				string v = m.Value;

				if (v.IsNullEmptyOrWhitespace())
					return v;

				if (v.Contains('\n'))
					v = v.Replace("\n", "]\n" + tagStart);
				
				return tagStart + v + "]";
			});

		}

		public static bool ObjectsNullEqual(object a, object b)
		{
			if (a is not null && b is not null) 
				return a.Equals(b);
			return a is null && b is null;
		}

		public static bool ObjectsNullEqual<T>(T a, T b, Func<T, T, bool> comparer)
		{
			if (a is not null && b is not null)
				return comparer(a, b);
			return a is null && b is null;
		}

		public static T CreateModItem<T>(int stack = 1) where T : ModItem 
		{
			Item item = new();
			item.SetDefaults(ModContent.ItemType<T>());
			item.stack = stack;

			return item.ModItem as T;
		}

		public static Vector2 WorldPixelsToScreen(Vector2 worldPos)
		{
			Vector2 screenHalf = Main.ScreenSize.ToVector2() / 2;
			Vector2 v = worldPos - Main.screenPosition - screenHalf;

			v *= Main.GameZoomTarget;

			return v + screenHalf;
		}

		public static Vector2 WorldToScreen(Point worldPos)
		{
			return WorldPixelsToScreen(worldPos.ToVector2() * 16);
		}

		public static byte[] WriteToByteArray(Action<BinaryWriter> action)
		{
			using MemoryStream ms = new MemoryStream();
			action(new(ms));
			return ms.ToArray();
		}

		public static T ReadFromByteArray<T>(byte[] array, Func<BinaryReader, T> func)
		{
			using MemoryStream ms = new MemoryStream(array);
			return func(new(ms));
		}

        public static string GetLangText(string key, string @default, object[] formatters)
        {
			string text = Language.Exists(key) ? Language.GetTextValue(key) : @default;
			if (text is not null && formatters is not null)
				text = string.Format(text, formatters);

			return text ?? "";
        }

		public static IEnumerable<T> Enum<T>(T obj0)
		{
			yield return obj0;
		}

		public static IEnumerable<T> Enum<T>(T obj0, T obj1)
		{
			yield return obj0;
			yield return obj1;
		}

		public static IEnumerable<T> Enum<T>(T obj0, T obj1, T obj2)
		{
			yield return obj0;
			yield return obj1;
			yield return obj2;
		}

		public static Item TryGetItemFromPlayerInventory(Func<Item, bool> matcher, int amount = 1)
		{
			IEnumerable<Item> items = Main.LocalPlayer.inventory;

			if (Main.LocalPlayer.chest > -1)
				items = items.Concat(Main.chest[Main.LocalPlayer.chest].item);
			
			else if (Main.LocalPlayer.chest == -2)
				items = items.Concat(Main.LocalPlayer.bank.item);
			
			else if (Main.LocalPlayer.chest == -3)
				items = items.Concat(Main.LocalPlayer.bank2.item);
			
			else if (Main.LocalPlayer.chest == -4)
				items = items.Concat(Main.LocalPlayer.bank3.item);
			
			else if (Main.LocalPlayer.chest == -5)
				items = items.Concat(Main.LocalPlayer.bank4.item);

			foreach (Item item in items)
				if (matcher(item) && item.stack >= amount)
				{
					Item clone = item.Clone();
					clone.stack = amount;
					item.stack -= amount;

					if (item.stack <= 0)
						item.TurnToAir();

					SoundEngine.PlaySound(SoundID.Grab);

					return clone;
				}

			return null;
		}
    }
}
