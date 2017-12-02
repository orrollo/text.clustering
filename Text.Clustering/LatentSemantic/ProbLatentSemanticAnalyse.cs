using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Text.Clustering.LatentSemantic
{
    public class ProbLatentSemanticAnalyse
    {
        ParallelOptions _options = new ParallelOptions() { MaxDegreeOfParallelism = Environment.ProcessorCount };

        protected Dictionary<int, Dictionary<int, int>> counts;

        private readonly Dictionary<int,int> _docNumbers = new Dictionary<int, int>();
        private readonly Dictionary<int, int> _wordNumbers = new Dictionary<int, int>();

        private int _docCount;
        private int _wordCount;
        private int _themeCount;

        public int ThemeCount {get { return _themeCount; } }

        private int[] _nd;
        private double[,] _phiWt;
        private double[,] _quDt;
        private double[,] _nWt;
        private double[,] _nDt;
        private double[] _nt;

        private Dictionary<string, int> _wordIndexes = null;

        public Dictionary<string, int> WordIndexes { get { return _wordIndexes; } }

        public ProbLatentSemanticAnalyse(int themeNumber, IEnumerable<IEnumerable<string>> texts)
        {
            _wordIndexes = new Dictionary<string, int>();
            int docIndex = 0;
            var data = new Dictionary<int,Dictionary<int,int>>();
            foreach (var text in texts)
            {
                data[docIndex] = new Dictionary<int, int>();
                foreach (var word in text)
                {
                    if (!_wordIndexes.ContainsKey(word)) _wordIndexes[word] = _wordIndexes.Count;
                    var wordIndex = _wordIndexes[word];
                    if (!data[docIndex].ContainsKey(wordIndex)) 
                        data[docIndex][wordIndex] = 1;
                    else
                        data[docIndex][wordIndex] = data[docIndex][wordIndex] + 1;
                }
                docIndex++;
            }
            PreprocessInput(themeNumber, data);
        }

        public ProbLatentSemanticAnalyse(int themeNumber, Dictionary<int, Dictionary<int, int>> data)
        {
            PreprocessInput(themeNumber, data);
        }

        private void PreprocessInput(int themeNumber, Dictionary<int, Dictionary<int, int>> data)
        {
            TransformData(data);
            //
            _themeCount = themeNumber;
            _docCount = _docNumbers.Count;
            _wordCount = _wordNumbers.Count;
            //
            _nd = new int[_docCount];
            for (int d = 0; d < _docCount; d++) foreach (var count in counts[d].Values) _nd[d] += count;
            //
            _nt = new double[_themeCount];
            _phiWt = new double[_wordCount, _themeCount];
            _quDt = new double[_docCount, _themeCount];
            _nWt = new double[_wordCount, _themeCount];
            _nDt = new double[_docCount, _themeCount];
            //
            var rnd = new Random();
            for (int t = 0; t < _themeCount; t++)
            {
                for (int w = 0; w < _wordCount; w++) _phiWt[w, t] = (rnd.NextDouble()*0.98) + 0.01;
                for (int d = 0; d < _docCount; d++) _quDt[d, t] = (rnd.NextDouble()*0.98) + 0.01;
            }
            NormPhi();
            NormQu();
        }

        private void NormQu()
        {
            Parallel.For(0, _docCount, _options, d =>
            {
                double sum = 0.0;
                for (int t = 0; t < _themeCount; t++) sum += _quDt[d, t];
                for (int t = 0; t < _themeCount; t++) _quDt[d, t] = _quDt[d, t] / sum;
            });

            //for (int d = 0; d < _docCount; d++)
            //{
            //    double sum = 0.0;
            //    for (int t = 0; t < _themeCount; t++) sum += _quDt[d, t];
            //    for (int t = 0; t < _themeCount; t++) _quDt[d, t] = _quDt[d, t] / sum;
            //}
        }

        private void NormPhi()
        {
            Parallel.For(0, _wordCount, _options, w =>
            {
                double sum = 0.0;
                for (int t = 0; t < _themeCount; t++) sum += _phiWt[w, t];
                for (int t = 0; t < _themeCount; t++) _phiWt[w, t] = _phiWt[w, t] / sum;
            });

            //for (int w = 0; w < _wordCount; w++)
            //{
            //    double sum = 0.0;
            //    for (int t = 0; t < _themeCount; t++) sum += _phiWt[w, t];
            //    for (int t = 0; t < _themeCount; t++) _phiWt[w, t] = _phiWt[w, t]/sum;
            //}
        }

        public void Train(int maxSteps = 100, Action<int> stepDelegate = null)
        {
            double r = 0.2;
            double prev_l = 0.0;
            for (int step = 0; step < maxSteps; step++)
            {
                for (int t = 0; t < _themeCount; t++)
                {
                    _nt[t] = 0;
                    for (int w = 0; w < _wordCount; w++) _nWt[w, t] = 0.0;
                    for (int d = 0; d < _docCount; d++) _nDt[d, t] = 0.0;
                }
                // 
                for (int d = 0; d < _docCount; d++)
                {
                    Parallel.For(0, _wordCount, _options, w =>
                    {
                    //for (int w = 0; w < _wordCount; w++)
                    //{
                        var ndw = getNdw(d, w);
                        double z = 0;
                        for (int t = 0; t < _themeCount; t++) z += _phiWt[w, t] * _quDt[d, t];
                        for (int t = 0; t < _themeCount; t++)
                        {
                            var value = _phiWt[w, t] * _quDt[d, t];
                            if (!(value > 0)) continue;
                            double chg = ndw*value/z;
                            _nWt[w, t] += chg;
                            _nDt[d, t] += chg;
                            _nt[t] += chg;
                        }
                    //}
                    });
                }
                //
                Parallel.For(0, _themeCount, _options, t =>
                {
                    //for (int t = 0; t < _themeCount; t++)
                    //{
                    for (int d = 0; d < _docCount; d++) _quDt[d, t] = _nDt[d, t]/_nd[d];
                    for (int w = 0; w < _wordCount; w++) _phiWt[w, t] = _nWt[w, t]/_nt[t];
                    //}
                });
                if (step >= 3) MakeSmallsAreZeros(1e-8);
                //
                NormPhi();
                NormQu();
                //
                var l = CalcLikehood();
                if (stepDelegate != null) stepDelegate(step);
                if (Math.Abs(l - prev_l) < 1e-5) break;
                prev_l = l;
            }
            MakeSmallsAreZeros(1e-5);
        }

        public double[,] PhiWt { get { return _phiWt; } }

        public double[,] QuDt { get { return _quDt; } }

        private void MakeSmallsAreZeros(double eps)
        {
            Parallel.For(0, _themeCount, _options, t =>
            {
                //for (int t = 0; t < _themeCount; t++)
                //{
                for (int w = 0; w < _wordCount; w++) if (_phiWt[w, t] < eps) _phiWt[w, t] = 0.0;
                for (int d = 0; d < _docCount; d++) if (_quDt[d, t] < eps) _quDt[d, t] = 0.0;
                //}
            });
        }

        private double CalcLikehood()
        {
            double l = 0.0;
            for (int d = 0; d < _docCount; d++)
            {
                for (int w = 0; w < _wordCount; w++)
                {
                    double sum = 0.0;
                    for (int t = 0; t < _themeCount; t++) sum += _phiWt[w, t]*_quDt[d, t];
                    l += getNdw(d, w)*Math.Log(sum);
                }
            }
            return l;
        }

        private int getNdw(int d, int w)
        {
            if (!counts.ContainsKey(d)) return 0;
            if (!counts[d].ContainsKey(w)) return 0;
            return counts[d][w];
        }

        private void TransformData(Dictionary<int, Dictionary<int, int>> data)
        {
            foreach (var pair in data)
            {
                if (pair.Value.Count == 0) continue;
                if (!_docNumbers.ContainsKey(pair.Key)) _docNumbers[pair.Key] = _docNumbers.Count;
                foreach (var wordNumber in pair.Value.Keys) if (!_wordNumbers.ContainsKey(wordNumber)) _wordNumbers[wordNumber] = _wordNumbers.Count;
            }
            counts = new Dictionary<int, Dictionary<int, int>>();
            foreach (var pair in data)
            {
                if (!_docNumbers.ContainsKey(pair.Key)) continue;
                var doc = _docNumbers[pair.Key];
                counts[doc] = new Dictionary<int, int>();
                foreach (var pair2 in pair.Value)
                {
                    var word = pair2.Key;
                    counts[doc][word] = pair2.Value;
                }
            }
        }
    }
}
