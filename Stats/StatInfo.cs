using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using ReLogic.Graphics;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerraIntegration.Basic;
using TerraIntegration.DataStructures;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;
using Terraria.UI.Chat;

namespace TerraIntegration.Stats
{
    public static class StatInfo
    {
        public const int HistoryCap = 200;

        static List<StatisticDisplayBase> Statistics = new();

        public static bool Visible;
        public static RingArray<string> Log = new(15);

        public static void Add(string name, Statistic stat)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException($"\"{nameof(name)}\" не может быть неопределенным или пустым.", nameof(name));
            }

            if (stat is null)
            {
                throw new ArgumentNullException(nameof(stat));
            }

            Statistics.Add(new StatisticDisplay(name, stat));
        }
        public static void Add<TName, TStat>(string name, Dictionary<TName, TStat> stat) where TStat : Statistic
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException($"\"{nameof(name)}\" не может быть неопределенным или пустым.", nameof(name));
            }

            if (stat is null)
            {
                throw new ArgumentNullException(nameof(stat));
            }

            Statistics.Add(new StatisticDictionaryDisplay<TName, TStat>(name, stat));
        }
        public static void AddDelimeter() 
        {
            Statistics.Add(new EmptyStatisticDisplay());
        }

        public static void LogMessage(string message)
        {
            if (Networking.Server || !TerraIntegration.DebugMode) return;
            Log.Push($"[c/aaaaaa:{DateTime.Now:HH:mm:ss}] {message}");
        }

        public static void Draw()
        {
            if (Main.keyState.IsKeyDown(Keys.Escape))
                Visible = false;

            if (!Visible) return;

            foreach (StatisticDisplayBase stat in Statistics)
                stat.Finish();

            string text = $"TerraIntegration stats ({HistoryCap} ticks sample):\n{string.Join('\n', Statistics.Select(s => s.Display()))}";

            ChatManager.DrawColorCodedStringWithShadow(Main.spriteBatch, FontAssets.MouseText.Value, text, new Vector2(20, 150), Color.White, 0f, Vector2.Zero, new(.8f));

            Point16 tile = (Point16)(Main.MouseWorld / 16);
            ComponentData data = ComponentWorld.Instance.GetDataOrNull(tile);
            if (data is not null && data.UpdateFrequency > 0 && ComponentWorld.Instance.ComponentUpdates.ContainsKey(tile))
            {
                ComponentWorld.Instance.AddHoverText($"{data.Component?.TypeName}\nFreq: {data.UpdateFrequency}\nUpdate: {TimeStatistic.FormatTime(data.LastUpdateTime)}");
            }
        }

        internal static void Unload() 
        {
            Statistics.Clear();
        }
    }
}
