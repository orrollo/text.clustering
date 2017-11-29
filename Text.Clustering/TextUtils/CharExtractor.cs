using Text.Clustering.Interface;

namespace Text.Clustering.TextUtils
{
    public class CharExtractor : ITextPartExtractor
    {
        protected static CharExtractor Singletone = new CharExtractor();

        public static CharExtractor Get
        {
            get { return Singletone; }
        }

        public uint GetPart(int index, string source)
        {
            if (string.IsNullOrEmpty(source) || index < 0 || index >= source.Length) return 0;
            return source[index];
        }
    }
}