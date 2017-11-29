using Text.Clustering.Interface;

namespace Text.Clustering.TextUtils
{
    public class NgramExtractor : ITextPartExtractor
    {
        protected int Size;

        public NgramExtractor(int size)
        {
            Size = size;
        }

        public uint GetPart(int index, string source)
        {
            if (string.IsNullOrEmpty(source)) return 0;
            unchecked
            {
                int len = (index + Size) > source.Length ? source.Length - index : Size;
                return (uint) source.Substring(index, len).GetHashCode();
            }
        }
    }
}