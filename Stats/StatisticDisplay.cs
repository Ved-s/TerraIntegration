using System.Collections.Generic;
using System.Linq;

namespace TerraIntegration.Stats
{
    abstract class StatisticDisplayBase
    {
        public string Name { get; }

        protected StatisticDisplayBase(string name)
        {
            Name = name;
        }

        public abstract void Finish();
        public abstract string Display();
    }

    class StatisticDictionaryDisplay<TName, TStat> : StatisticDisplayBase where TStat : Statistic
    {
        public Dictionary<TName, TStat> Dictionary { get; }

        public StatisticDictionaryDisplay(string name, Dictionary<TName, TStat> statDict) : base(name)
        {
            Dictionary = statDict;
        }
        public override void Finish()
        {
            foreach (Statistic stat in Dictionary.Values)
                stat.Finish();
        }
        public override string Display() => $"{Name}:\n{string.Join("\n", Dictionary.Select(kvp => $"  {kvp.Key}: {kvp.Value.DisplayString()}"))}";
    }

    class StatisticDisplay : StatisticDisplayBase
    {
        public Statistic Statistic { get; }

        public StatisticDisplay(string name, Statistic statistic) : base(name)
        {
            Statistic = statistic;
        }
        public override void Finish()
        {
            Statistic.Finish();
        }
        public override string Display() => Name + ": " + Statistic.DisplayString();
    }

    class EmptyStatisticDisplay : StatisticDisplayBase
    {
        public EmptyStatisticDisplay() : base(string.Empty) { }

        public override string Display() => string.Empty;

        public override void Finish() { }
    }
}
