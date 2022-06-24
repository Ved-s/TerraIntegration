using System;
using System.Diagnostics;
using System.Linq;
using TerraIntegration.DataStructures;

namespace TerraIntegration.Stats
{
    public abstract class Statistic
    {
        public abstract string DisplayString();
        public abstract void Finish();
    }

    public abstract class Statistic<T> : Statistic
    {
        public RingArray<T> ValueHistory { get; } = new(StatInfo.HistoryCap);

        public abstract T GetMean();
        public abstract T GetMax();

        public virtual string Format(T obj) => obj.ToString();
        public override string DisplayString()
        {
            if (ValueHistory.Length == 0)
                return "No data";
            return $"{Format(GetMean())} mean, {Format(GetMax())} max";
        }
    }

    public class CountStatistic : Statistic<int>
    {
        public int Current { get; private set; }

        public void Increase()
        {
            Current++;
        }

        public override int GetMean() => ValueHistory.Enumerate().Sum() / ValueHistory.Length;
        public override int GetMax() => ValueHistory.Enumerate().Max();

        public override void Finish()
        {
            ValueHistory.Push(Current);
            Current = 0;
        }
    }

    public class TimeStatistic : Statistic<TimeSpan>
    {
        public Stopwatch Watch { get; } = new();

        public void Start()
        {
            Watch.Start();
        }

        public void Stop()
        {
            Watch.Stop();
        }

        public override TimeSpan GetMax() => ValueHistory.Enumerate().Max();
        public override TimeSpan GetMean() => ValueHistory.Enumerate().Aggregate((a, b) => a + b) / ValueHistory.Length;

        public override void Finish()
        {
            ValueHistory.Push(Watch.Elapsed);
            Watch.Reset();
        }

        public override string Format(TimeSpan ts)
        {
            return FormatTime(ts);
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
    }
}
