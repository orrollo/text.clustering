using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Text.Clustering.Utils;

namespace Text.Clustering.VectorSpace
{
    public abstract class DocumentBase : IComparable<Document>
    {
        protected Dictionary<string,int> TermFrequency = new Dictionary<string, int>();
        protected HashSet<String> Terms = null;
        public int ProcessedTextHash { get; set; }

        protected DocumentBase(string source)
        {
            PreProcessText(source);
        }

        protected DocumentBase(Stream source, Encoding encoding)
        {
            PreProcessStream(source, encoding);
        }

        public void PreProcessStream(Stream stream, Encoding encoding = null)
        {
            if (stream == null) throw new ArgumentException();
            if (encoding == null) encoding = Encoding.UTF8;
            using (var rdr = new StreamReader(stream,encoding))
            {
                while (!rdr.EndOfStream)
                {
                    var ws = rdr.ReadLine() ?? string.Empty;
                    if (!string.IsNullOrEmpty(ws)) PreProcessText(ws);
                }
            }
        }

        public void PreProcessText(string source)
        {
            if (source == null) throw new ArgumentException();
            var scanner = new Scanner(source);
            while (scanner.HasNext())
            {
                var nextWord = scanner.Next() ?? string.Empty;
                string filteredWord = FilterWord(nextWord);
                if (string.IsNullOrEmpty(filteredWord)) continue;
                if (!TermFrequency.ContainsKey(filteredWord))
                {
                    Terms = null;
                    TermFrequency[filteredWord] = 1;
                }
                else
                {
                    TermFrequency[filteredWord] = TermFrequency[filteredWord] + 1;
                }
            }
            ProcessedTextHash = ProcessedTextHash ^ source.GetHashCode();
        }

        protected abstract string FilterWord(string nextWord);

        public double GetTermFrequency(String word)
        {
            return TermFrequency.ContainsKey(word) ? TermFrequency[word] : 0;
        }

        public HashSet<String> GetTerms()
        {
            return Terms ?? (Terms = new HashSet<string>(TermFrequency.Keys));
        }

        public int CompareTo(Document other)
        {
            return ProcessedTextHash.CompareTo(other.ProcessedTextHash);
        }

        public override string ToString()
        {
            return string.Format("hash={0};words={1}", ProcessedTextHash, TermFrequency.Count);
        }
    }
}