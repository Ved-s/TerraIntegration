using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TerraIntegration.Stats
{
    public static class Statistics
    {
        public static Dictionary<string, TimeStatistic> ComponentUpdates { get; } = new();

        public static TimeStatistic FullUpdate { get; } = new();
        public static TimeStatistic VariableRequests { get; } = new();
        public static TimeStatistic ComponentRequests { get; } = new();
        public static TimeStatistic ComponentDataRequests { get; } = new();
        public static TimeStatistic SearchingComponentData { get; } = new();

        public static CountStatistic ComponentsUpdated { get; } = new();
        public static CountStatistic VariablesRequested { get; } = new();
        public static CountStatistic ComponentsRequested { get; } = new();
        public static CountStatistic ComponentDatasRequested { get; } = new();

        public static TimeStatistic GetComponent(string type)
        {
            if (!ComponentUpdates.TryGetValue(type, out var stat))
            {
                stat = new();
                ComponentUpdates.Add(type, stat);
            }
            return stat;
        }

        internal static void Load()
        {
            StatInfo.Add("Full update", FullUpdate);
            StatInfo.Add("Component requests", ComponentRequests);
            StatInfo.Add("Component data requests", ComponentDataRequests);
            StatInfo.Add("Searching for data", SearchingComponentData);
            StatInfo.Add("Variable requests", VariableRequests);
            StatInfo.AddDelimeter();
            StatInfo.Add("Components updated", ComponentsUpdated);
            StatInfo.Add("Components requested", ComponentsRequested);
            StatInfo.Add("Component datas requested", ComponentDatasRequested);
            StatInfo.Add("Variables requested", VariablesRequested);
            StatInfo.AddDelimeter();
            StatInfo.Add("Component updates", ComponentUpdates);
        }
    }
}
