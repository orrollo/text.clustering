using System.IO;
using System.Text;

namespace Text.Clustering.Utils
{
    /// <summary>
    /// based on BlueMonkMN source (https://stackoverflow.com/questions/722270/is-there-an-equivalent-to-the-scanner-class-in-c-sharp-for-strings)
    /// </summary>
    public class Scanner : StringReader
    {
        private readonly StringBuilder _wordBuilder = new StringBuilder();
        private string _currentWord;

        public Scanner(string s) : base(s)
        {
            ReadNextWord();
        }

        private void ReadNextWord()
        {
            _wordBuilder.Clear();
            do
            {
                var next = Read();
                if (next < 0) break;
                var nextChar = (char)next;
                if (char.IsWhiteSpace(nextChar)) break;
                _wordBuilder.Append(nextChar);
            } while (true);
            while ((Peek() >= 0) && (char.IsWhiteSpace((char)Peek()))) Read();
            _currentWord = _wordBuilder.Length > 0 ? _wordBuilder.ToString() : null;
        }

        public bool HasNextInt()
        {
            if (_currentWord == null)
                return false;
            int dummy;
            return int.TryParse(_currentWord, out dummy);
        }

        public int NextInt()
        {
            try
            {
                return int.Parse(_currentWord);
            }
            finally
            {
                ReadNextWord();
            }
        }

        public bool HasNextDouble()
        {
            if (_currentWord == null)
                return false;
            double dummy;
            return double.TryParse(_currentWord, out dummy);
        }

        public double NextDouble()
        {
            try
            {
                return double.Parse(_currentWord);
            }
            finally
            {
                ReadNextWord();
            }
        }

        public string Next()
        {
            try
            {
                return _currentWord;
            }
            finally
            {
                ReadNextWord();
            }
        }


        public bool HasNext()
        {
            return _currentWord != null;
        }
    }
}