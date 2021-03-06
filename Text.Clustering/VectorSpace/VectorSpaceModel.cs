using System;
using System.Collections.Generic;
using System.Linq;

namespace Text.Clustering.VectorSpace
{
    public class VectorSpaceModel
    {
        private readonly Corpus _corpus;

        private readonly Dictionary<DocumentBase, Dictionary<string, double>> _tfIdfWeights;

        public VectorSpaceModel(Corpus corpus)
        {
            _corpus = corpus;
            _tfIdfWeights = new Dictionary<DocumentBase, Dictionary<string, double>>();
            CreateTfIdfWeights();
        }

        private void CreateTfIdfWeights()
        {
            var terms = _corpus.InvertedIndex.Keys;
            foreach (var document in _corpus.Documents)
            {
                var weights = new Dictionary<string, double>();
                foreach (var term in terms)
                {
                    double tf = document.GetTermFrequency(term);
                    double idf = _corpus.GetInverseDocumentFrequency(term);
                    weights[term] = tf*idf;
                }
                _tfIdfWeights[document] = weights;
            }
        }

        private Dictionary<DocumentBase,double> magnitudes = new Dictionary<DocumentBase, double>();

        private double GetMagnitude(DocumentBase document)
        {
            if (!magnitudes.ContainsKey(document))
            {
                var weights = _tfIdfWeights[document];
                var magnitude = weights.Values.Sum(weight => weight * weight);
                magnitudes[document] = Math.Sqrt(magnitude);
            }
            return magnitudes[document];
        }

        protected Dictionary<DocumentBase,Dictionary<DocumentBase,double>> cache = new Dictionary<DocumentBase, Dictionary<DocumentBase, double>>();

        public Dictionary<string, double> GetWeights(DocumentBase document)
        {
            return _tfIdfWeights.ContainsKey(document) ? _tfIdfWeights[document] : null;
        }

        private double GetDotProduct(DocumentBase d1, DocumentBase d2)
        {
            if (cache.ContainsKey(d1) && cache[d1].ContainsKey(d2)) return cache[d1][d2];
            if (cache.ContainsKey(d2) && cache[d2].ContainsKey(d1)) return cache[d2][d1];

            var weights1 = _tfIdfWeights[d1];
            var weights2 = _tfIdfWeights[d2];
            double sum = 0;
            foreach (var term in weights1.Keys)
            {
                var w1 = weights1[term];
                var w2 = weights2[term];
                sum += w1*w2;
            }

            if (!cache.ContainsKey(d1)) cache[d1] = new Dictionary<DocumentBase, double>();
            if (!cache[d1].ContainsKey(d2)) cache[d1][d2] = sum;

            return sum;
        }

        public double CosineSimilarity(Document d1, Document d2)
        {
            return GetDotProduct(d1, d2) / (GetMagnitude(d1) * GetMagnitude(d2));
        }
    }
}