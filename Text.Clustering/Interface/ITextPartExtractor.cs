namespace Text.Clustering.Interface
{
    public interface ITextPartExtractor
    {
        uint GetPart(int index, string source);
    }
}