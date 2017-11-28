using System;
using System.Collections.Generic;
using System.Linq;

namespace Text.Clustering.VectorSpace
{
    public class Corpus
    {
        private readonly IEnumerable<DocumentBase> _documents;
        private readonly Dictionary<string, HashSet<DocumentBase>> _invertedIndex;

        public IEnumerable<DocumentBase> Documents
        {
            get { return _documents; }
        }

        public Dictionary<string, HashSet<DocumentBase>> InvertedIndex
        {
            get { return _invertedIndex; }
        }

        public Corpus(IEnumerable<Document> documents)
        {
            this._documents = documents;
            _invertedIndex = new Dictionary<string, HashSet<DocumentBase>>();
            CreateInvertedIndex();
        }

        private void CreateInvertedIndex()
        {
            foreach (var document in _documents)
            {
                var terms = document.GetTerms();
                foreach (var term in terms)
                {
                    if (!_invertedIndex.ContainsKey(term)) _invertedIndex[term] = new HashSet<DocumentBase>();
                    _invertedIndex[term].Add(document);
                }
            }
        }

        public double GetInverseDocumentFrequency(string term)
        {
            if (!_invertedIndex.ContainsKey(term)) return 0;
            double size = _documents.Count();
            double documentFrequency = _invertedIndex[term].Count;
            return Math.Log10(size/documentFrequency);
        }
    }
}