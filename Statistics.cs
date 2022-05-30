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
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace TerraIntegration
{
    public static class Statistics
    {
        public static Stopwatch[] Updates = new Stopwatch[4];
        public static TimeSpan[] MaxUpdates = new TimeSpan[4];
        public static bool Visible;
        public static RingArray<string> Log = new(15);

        public static int
            UpdatedComponents = 0,
            VariableRequests = 0,
            ComponentRequests = 0;

        public static int Counter = 0;

        public static void ResetUpdates()
        {
            for (int i = 0; i < Updates.Length; i++)
            {
                if (Updates[i] is null) 
                    Updates[i] = new Stopwatch();
                else Updates[i].Reset();
            }

            UpdatedComponents = 0;
            VariableRequests = 0;
            ComponentRequests = 0;

            Counter++;
            if (Counter % 60 == 0)
            {
                Counter = 0;
                for (int i = 0; i < MaxUpdates.Length; i++)
                    MaxUpdates[i] = default;
            }
        }

        public static void LogMessage(string message)
        {
            if (Networking.Server) return;
            Log.Push($"[{DateTime.Now:HH:mm:ss}] {message}");
        }

        public static string Get(UpdateTime time)
        {
            int timei = (int)time;

            if (Updates[timei] is null) return null;

            TimeSpan ts = Updates[timei].Elapsed;

            if (MaxUpdates[timei] < ts)
                MaxUpdates[timei] = ts;
            else ts = MaxUpdates[timei];
            

            double micro = ts.TotalMilliseconds * 1000;

            if (micro < 1000) return $"{micro:0.0}us max";

            if (ts.TotalMilliseconds < 10) return $"{ts.TotalMilliseconds:0.000}ms max";
            if (ts.TotalMilliseconds < 100) return $"{ts.TotalMilliseconds:0.00}ms max";
            if (ts.TotalMilliseconds < 1000) return $"{ts.TotalMilliseconds:0.0}ms max";

            if (ts.TotalSeconds < 10) return $"{ts.TotalSeconds:0.000}s max";
            if (ts.TotalSeconds < 100) return $"{ts.TotalSeconds:0.00}s max";
            if (ts.TotalSeconds < 1000) return $"{ts.TotalSeconds:0.0}s max";

            return ts.ToString();

        }

        public static void Start(UpdateTime time) 
        {
            Updates[(int)time].Start();
        }

        public static void Stop(UpdateTime time)
        {
            Updates[(int)time].Stop();
        }

        public static void Draw()
        {
            if (Main.keyState.IsKeyDown(Keys.Escape))
                Visible = false;

            if (!Visible) return;

            int width = 800;
            int height = 600;

            Rectangle rect = new((Main.screenWidth - width) / 2, (Main.screenHeight - height) / 2, width, height);
            Main.spriteBatch.Draw(TextureAssets.MagicPixel.Value, rect, Color.DarkSlateBlue * .7f);

            string text = $"TerraIntegration stats:\n" +
                $"Full update: {Get(UpdateTime.FullUpdate)}\n" +
                $"Components: {UpdatedComponents} in {Get(UpdateTime.Components)}\n" +
                $"Component requests: {ComponentRequests} in {Get(UpdateTime.ComponentRequests)}\n" +
                $"Variable requests: {VariableRequests} in {Get(UpdateTime.VariableRequests)}";

            Main.spriteBatch.DrawString(FontAssets.MouseText.Value, text, new Vector2(rect.X + 20, rect.Y + 20), Color.White);

            Main.spriteBatch.DrawString(FontAssets.MouseText.Value, $"Log:\n{string.Join("\n", Log.EnumerateBackwards())}", new Vector2(rect.X + 20, rect.Y + 180), Color.White, 0f, Vector2.Zero, .8f, SpriteEffects.None, 0f);
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
