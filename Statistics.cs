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

namespace TerraIntegration
{
    public static class Statistics
    {
        const int UpdateTypes = 4;
        public static Stopwatch[] Updates = new Stopwatch[UpdateTypes];
        public static Dictionary<string, Stopwatch> ComponentUpdates = new();

        const int MeanCap = 200;
        public static RingArray<TimeSpan>[] UpdateHistory = new RingArray<TimeSpan>[UpdateTypes];
        public static Dictionary<string, RingArray<TimeSpan>> ComponentUpdateHistory = new();

        public static bool Visible;
        public static RingArray<string> Log = new(15);

        public static int
            UpdatedComponents = 0,
            VariableRequests = 0,
            ComponentRequests = 0;

        public static void ResetUpdates()
        {
            UpdatedComponents = 0;
            VariableRequests = 0;
            ComponentRequests = 0;

            for (int i = 0; i < UpdateHistory.Length; i++)
            {
                RingArray<TimeSpan> updates = UpdateHistory[i];
                if (updates is null)
                    UpdateHistory[i] = updates = new(MeanCap);

                if (Updates[i] is not null)
                    updates.Push(Updates[i].Elapsed);
            }

            foreach (var kvp in ComponentUpdates)
            {
                if (!ComponentUpdateHistory.TryGetValue(kvp.Key, out var com))
                    ComponentUpdateHistory[kvp.Key] = com = new(MeanCap);
                com.Push(kvp.Value.Elapsed);
            }

            for (int i = 0; i < Updates.Length; i++)
            {
                if (Updates[i] is null)
                    Updates[i] = new Stopwatch();
                else Updates[i].Reset();
            }

            foreach (Stopwatch watch in ComponentUpdates.Values)
                watch.Reset();
        }

        public static void LogMessage(string message)
        {
            if (Networking.Server || !TerraIntegration.DebugMode) return;
            Log.Push($"[c/aaaaaa:{DateTime.Now:HH:mm:ss}] {message}");
        }

        public static string Get(UpdateTime time)
        {
            int timei = (int)time;

            RingArray<TimeSpan> updates = UpdateHistory[timei];

            if (Updates[timei] is null || updates is null)
                return null;

            TimeSpan mean = updates.Enumerate().Aggregate((a, b) => a + b) / MeanCap;
            TimeSpan max = updates.Enumerate().Aggregate((a, b) => a > b ? a : b);

            return $"{FormatTime(mean)} mean, {FormatTime(max)} max";
        }
        public static void Start(UpdateTime time)
        {
            Updates[(int)time].Start();
        }
        public static void Stop(UpdateTime time)
        {
            Updates[(int)time].Stop();
        }

        public static void StartComponent(string type)
        {
            if (!ComponentUpdates.TryGetValue(type, out Stopwatch watch))
                ComponentUpdates.Add(type, watch = new());
            Start(UpdateTime.Components);
            watch.Start();
        }
        public static void StopComponent(string type)
        {
            Stop(UpdateTime.Components);
            if (!ComponentUpdates.TryGetValue(type, out Stopwatch watch))
                return;
            watch.Stop();
        }
        public static string GetComponent(string type) 
        {
            if (!ComponentUpdateHistory.TryGetValue(type, out var hist))
                return null;

            TimeSpan mean = hist.Enumerate().Aggregate((a, b) => a + b) / MeanCap;
            TimeSpan max = hist.Enumerate().Aggregate((a, b) => a > b ? a : b);

            return $"  {type}: {FormatTime(mean)} mean, {FormatTime(max)} max";
        }

        public static string FormatTime(TimeSpan ts)
        {
            double micro = ts.TotalMilliseconds * 1000;

            if (micro < 1000) return $"{micro:0.0}us";

            if (ts.TotalMilliseconds < 10) return $"{ts.TotalMilliseconds:0.000}ms";
            if (ts.TotalMilliseconds < 100) return $"{ts.TotalMilliseconds:0.00}ms";
            if (ts.TotalMilliseconds < 1000) return $"{ts.TotalMilliseconds:0.0}ms";

            if (ts.TotalSeconds < 10) return $"{ts.TotalSeconds:0.000}s";
            if (ts.TotalSeconds < 100) return $"{ts.TotalSeconds:0.00}s";
            if (ts.TotalSeconds < 1000) return $"{ts.TotalSeconds:0.0}s";

            return ts.ToString();
        }

        public static void Draw()
        {
            if (Main.keyState.IsKeyDown(Keys.Escape))
                Visible = false;

            if (!Visible) return;

            string text = $"TerraIntegration stats ({MeanCap} ticks sample):\n" +
                $"Full update: {Get(UpdateTime.FullUpdate)}\n" +
                $"Component requests: {ComponentRequests}\n  total: {Get(UpdateTime.ComponentRequests)}\n" +
                $"Variable requests: {VariableRequests}\n  total: {Get(UpdateTime.VariableRequests)}\n" + 
                $"Component updates: {UpdatedComponents}\n  total: {Get(UpdateTime.Components)}\n" +
                    $"{string.Join('\n', ComponentUpdateHistory.Keys.Select(t => GetComponent(t)))}" +
                (TerraIntegration.DebugMode ? $"\n\nLog:\n{string.Join("\n", Log.EnumerateBackwards())}" : "");

            ChatManager.DrawColorCodedStringWithShadow(Main.spriteBatch, FontAssets.MouseText.Value, text, new Vector2(20, 150), Color.White, 0f, Vector2.Zero, new(.8f));

            Point16 tile = (Point16)(Main.MouseWorld / 16);
            ComponentData data = ComponentWorld.Instance.GetDataOrNull(tile);
            if (data is not null && data.UpdateFrequency > 0 && ComponentWorld.Instance.ComponentUpdates.ContainsKey(tile))
            {
                ComponentWorld.Instance.AddHoverText($"{data.Component?.TypeName}\nFreq: {data.UpdateFrequency}\nUpdate: {FormatTime(data.LastUpdateTime)}");
            }
        }

        public enum UpdateTime 
        {
            FullUpdate,
            Components,
            ComponentRequests,
            VariableRequests
        }

        public class RingArray<T>
        {
            public T[] Values;
            int Index, Count;

            public int Length => Count;

            public RingArray(int capacity) 
            {
                Values = new T[capacity];
            }

            int ConvertIndex(int index)
            {
                index += Index;
                index %= Values.Length;
                if (index < 0)
                    index += Values.Length;
                return index;
            }

            public T this[int i] 
            {
                get => Values[ConvertIndex(i)];
                set => Values[ConvertIndex(i)] = value;
            }

            public void Push(T value)
            {
                Values[Index] = value;
                Index = (Index + 1) % Values.Length;
                if (Count < Values.Length) 
                    Count++;
            }

            public T Pop()
            {
                if (Count <= 0) 
                    return default;

                Index = ConvertIndex(-1);
                Count--;
                return Values[Index];
            }

            public IEnumerable<T> Enumerate()
            {
                for (int i = -(Count - 1); i < 0; i++)
                    yield return Values[ConvertIndex(i)];
            }

            public IEnumerable<T> EnumerateBackwards()
            {
                for (int i = 1; i <= Count; i++)
                    yield return Values[ConvertIndex(-i)];
            }
        }
    }
}
