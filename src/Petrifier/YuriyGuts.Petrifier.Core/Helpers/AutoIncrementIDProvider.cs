using System.Collections.Generic;

namespace YuriyGuts.Petrifier.Core.Helpers
{
    public class AutoIncrementIDProvider
    {
        private Dictionary<string, int> idCounters;

        public AutoIncrementIDProvider()
        {
            idCounters = new Dictionary<string, int>();
        }

        public int GetNextID(string key)
        {
            if (!idCounters.ContainsKey(key))
            {
                idCounters.Add(key, 0);
            }
            return ++idCounters[key];
        }

        public void Reset()
        {
            idCounters.Clear();
        }
    }
}
