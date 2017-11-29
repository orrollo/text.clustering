using System;
using System.Collections.Generic;
using System.Linq;

namespace Text.Clustering.Hashing
{
    public class LocalitySensitiveHashing
    {
        private readonly MinHashing _hashing;

        private readonly int _hashesInGroup;
        private readonly int _groupCount;

        public LocalitySensitiveHashing(int hashesInGroup, int groupCount)
        {
            _hashesInGroup = hashesInGroup;
            _groupCount = groupCount;
            _hashing = new MinHashing(_hashesInGroup * _groupCount);
        }

        public LocalitySensitiveHashing(double minSim, double maxSim, double minProb, double maxProb)
        {
            int hashesInGroup = -1;
            var k = Math.Log(1.0 - minProb)/Math.Log(1.0 - maxProb);
            double oldFn, fn = 1.0;
            for (int tr = 1; tr < 40; tr++)
            {
                oldFn = fn;
                fn = (Math.Log(1.0 - Math.Pow(minSim, tr)) / Math.Log(1.0 - Math.Pow(maxSim, tr))) - k;
                if (tr == 1) continue;
                if (fn > 0) continue;
                hashesInGroup = -fn > oldFn ? tr - 1 : tr;
                break;
            }
            if (hashesInGroup == -1) throw new ArgumentException();
            //
            double groupCount = Math.Log(1.0 - maxProb) / Math.Log(1.0 - Math.Pow(maxSim, hashesInGroup));
            //
            _hashesInGroup = hashesInGroup;
            _groupCount = (int) (groupCount + 0.9999);
            // 
            _hashing = new MinHashing(_hashesInGroup*_groupCount);
        }

        public int HashesInGroup
        {
            get { return _hashesInGroup; }
        }

        public int GroupCount
        {
            get { return _groupCount; }
        }

        public MinHashing Hashing
        {
            get { return _hashing; }
        }

        public void Process(int textIndex, string text)
        {
            IntProcessDocument(textIndex, text, (groupIndex, hashValue, idx) =>
            {
                if (!Groups.ContainsKey(groupIndex)) Groups[groupIndex] = new Dictionary<uint, HashSet<int>>();
                if (!Groups[groupIndex].ContainsKey(hashValue)) Groups[groupIndex][hashValue] = new HashSet<int>();
                Groups[groupIndex][hashValue].Add(idx);
            });
        }

        public int[] FindSimiliar(string text)
        {
            var ret = new HashSet<int>();
            IntProcessDocument(0, text, (groupIndex, hashValue, dummy) =>
            {
                if (!Groups.ContainsKey(groupIndex)) return;
                if (!Groups[groupIndex].ContainsKey(hashValue)) return;
                foreach (var idx in Groups[groupIndex][hashValue]) ret.Add(idx);
            });
            var array = ret.ToArray();
            Array.Sort(array);
            return array;
        }

        private void IntProcessDocument(int textIndex, string text, Action<int, uint, int> proc)
        {
            int currentIndex = 0;
            var hash = _hashing.Calculate(text);
            for (int groupIndex = 0; groupIndex < _groupCount; groupIndex++)
            {
                uint val = 0;
                for (int hashIndex = 0; hashIndex < _hashesInGroup; hashIndex++, currentIndex++) val ^= hash[currentIndex];
                proc(groupIndex, val, textIndex);
            }
        }

        protected Dictionary<int,Dictionary<uint,HashSet<int>>> Groups = new Dictionary<int, Dictionary<uint, HashSet<int>>>();
    }
}