using System;
using Text.Clustering.Interface;
using Text.Clustering.TextUtils;

namespace Text.Clustering.Hashing
{
    public class MinHashing
    {
        public delegate int HashDelegate(int src);

        protected uint[] koefA = null;
        protected uint[] koefB = null;
        protected uint Mod;

        public ITextPartExtractor Extractor
        {
            get { return _extractor; }
            set { _extractor = value ?? CharExtractor.Get; }
        }

        protected uint NextUInt(Random rnd)
        {
            return (uint) (rnd.Next() & 0xffffff);
        }

        protected static uint Gcd(uint a, uint b)
        {
            if (b > a) return Gcd(b, a);
            if (b == 0) throw new ArgumentException();
            if (b == 1) return 1;
            while (true)
            {
                uint c = a%b;
                if (c == 0) return b;
                a = b;
                b = c;
            }
        }

        public MinHashing(int size, uint mod = 10000000)
        {
            Mod = mod;
            koefA = new uint[size];
            koefB = new uint[size];
            var rnd = new Random();
            for (int i = 0; i < size; i++)
            {
                unchecked
                {
                    uint a, b;
                    do
                    {
                        a = (NextUInt(rnd) << 3) + 5;
                        b = (NextUInt(rnd) << 1) + 1;
                    } while (Gcd(a, b) != 1);
                    koefA[i] = a;
                    koefB[i] = b;
                }
            }
        }

        public uint[] Calculate(string source, uint[] hash = null)
        {
            if (hash == null)
            {
                hash = new uint[koefA.Length];
                for (int i = 0; i < hash.Length; i++) hash[i] = uint.MaxValue;
            }
            if (string.IsNullOrEmpty(source)) return hash;
            for (int i = 0; i < hash.Length; i++)
            {
                for (int index = 0; index < source.Length; index++)
                {
                    uint ch = _extractor.GetPart(index, source);
                    unchecked
                    {
                        uint value = ((ch + 17)*koefA[i] + koefB[i]) % Mod;
                        if (hash[i] > value) hash[i] = value;
                    }
                }
            }
            return hash;
        }

        private ITextPartExtractor _extractor = CharExtractor.Get;
    }
}
