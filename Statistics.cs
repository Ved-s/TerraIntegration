using Microsoft.Xna.Framework;
using ReLogic.Graphics;
using System;
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
    public class Statistics : ILoadable
    {
        public static Stopwatch[] Updates = new Stopwatch[4];
        public static TimeSpan[] MaxUpdates = new TimeSpan[4];

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


        public void Load(Mod mod)
        {
            On.Terraria.Main.DrawInterface_18_DiagnoseVideo += OnDrawVideoDiag;
        }

        public void Unload()
        {
            On.Terraria.Main.DrawInterface_18_DiagnoseVideo -= OnDrawVideoDiag;
        }

        private void OnDrawVideoDiag(On.Terraria.Main.orig_DrawInterface_18_DiagnoseVideo orig)
        {
            orig();

            if (!Main.drawDiag) return;

            string text = $"TerraIntegration stats:\n" +
                $"Full update: {Get(UpdateTime.FullUpdate)}\n" +
                $"Components: {UpdatedComponents} in {Get(UpdateTime.Components)}\n" +
                $"Component requests: {ComponentRequests} in {Get(UpdateTime.ComponentRequests)}\n" +
                $"Variable requests: {VariableRequests} in {Get(UpdateTime.VariableRequests)}";

            Vector2 size = FontAssets.MouseText.Value.MeasureString(text);

            Vector2 pos = new Vector2(20, Main.screenHeight - size.Y);

            if (Main.showFrameRate) pos.Y -= 25;

            Main.spriteBatch.DrawString(FontAssets.MouseText.Value, text, pos, Color.White);
        }

        public enum UpdateTime 
        {
            FullUpdate,
            Components,
            ComponentRequests,
            VariableRequests
        }
    }
}
