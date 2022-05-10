using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TerraIntegration
{
    public class ShortGuids
    {
        private int DefaultGuidStringLength;

        private Dictionary<Guid, string> GuidToStr = new Dictionary<Guid, string>();

        public ShortGuids(int minShortLength = 3) 
        {
            DefaultGuidStringLength = minShortLength;
        }

        public string GetShortGuid(Guid guid)
        {
            AddToDictionary(guid);
            return GuidToStr[guid];
        }
        public void AddToDictionary(Guid guid)
        {
            if (GuidToStr.ContainsKey(guid))
                return;

            Guid? concurrent = null;
            int concurrentSim = DefaultGuidStringLength;
            foreach (KeyValuePair<Guid, string> kvp in GuidToStr)
            {
                int sim = GetGuidSimilarity(kvp.Key, guid);
                if (sim >= concurrentSim)
                {
                    concurrentSim = sim;
                    concurrent = kvp.Key;
                }
            }
            if (concurrent.HasValue)
            {
                GuidToStr[concurrent.Value] = GetGuidString(concurrent.Value, concurrentSim + 1);
                GuidToStr.Add(guid, GetGuidString(guid, concurrentSim + 1));
            }
            else GuidToStr.Add(guid, GetGuidString(guid, DefaultGuidStringLength));
        }
        public Guid? GetGuid(string str) 
        {
            foreach (KeyValuePair<Guid, string> kvp in GuidToStr) 
            {
                if (kvp.Value.StartsWith(str))
                    return kvp.Key;
            }
            return null;
        }
        public void Clear()
        {
            GuidToStr.Clear();
        }
        public Guid? GetUniqueId(string startingWith)
        {
            int length = startingWith.Replace("-", "").Length;
            while (true)
            {
                if (length == 32)
                {
                    if (GetGuid(startingWith).HasValue) return null;
                    return Guid.Parse(startingWith);
                }

                List<char> chars = new("0123456789abcdef");
                foreach (KeyValuePair<Guid, string> kvp in GuidToStr)
                {
                    if (kvp.Value.StartsWith(startingWith))
                    {
                        chars.Remove(GetGuidChar(kvp.Key, startingWith.Length));
                    }
                }

                if (chars.Count == 0)
                {
                    chars = new("0123456789abcdef");
                }
                int ind = Random.Shared.Next(0, chars.Count);
                startingWith += chars[ind];
                length++;

                if (length == 8 || length == 12 || length == 16 || length == 20)
                    startingWith += '-';

            }
        }

        private static int GetGuidSimilarity(Guid a, Guid b) 
        {
            byte[] abytes = a.ToByteArray();
            byte[] bbytes = b.ToByteArray();

            int sim = 0;
            for (int i = 0; i < 16; i++)
            {
                int arrindex = 0;
                if (i < 4) arrindex = 3 - i;
                else if (i < 6) arrindex = (1 - (i - 4)) + 4;
                else if (i < 8) arrindex = (1 - (i - 6)) + 6;
                else arrindex = i;

                if (abytes[arrindex] == bbytes[arrindex])
                    sim += 2;
                else if ((abytes[arrindex] & 0xf0) == (bbytes[arrindex] & 0xf0))
                {
                    sim++;
                    break;
                }
                else break;
            }
            return sim;
        }
        private static string GetGuidString(Guid g, int digits) 
        {
            string gstr = g.ToString();
            int dashes = 0;

            if (digits >= 8) dashes++;
            if (digits >= 12) dashes++;
            if (digits >= 16) dashes++;
            if (digits >= 20) dashes++;

            return gstr.Substring(0, digits + dashes);
        }
        private static char GetGuidChar(Guid g, int index)
        {
            return g.ToString("N")[index];
        }
    }
}
