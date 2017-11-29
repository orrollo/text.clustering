using System.Text.RegularExpressions;
using Text.Clustering.Interface;

namespace Text.Clustering.TextUtils
{
    public class DefaultTextPreprocessor : ITextPreprocessor
    {
        protected static DefaultTextPreprocessor Singletone = new DefaultTextPreprocessor();

        protected Regex ReplaceRegex = new Regex(@"[^a-zA-Zа-яА-Я0-9]", RegexOptions.Compiled | RegexOptions.CultureInvariant);

        public static DefaultTextPreprocessor Get
        {
            get { return Singletone; }
        }

        public string Prepare(string src)
        {
            return string.IsNullOrEmpty(src) ? src : ReplaceRegex.Replace(src, "").ToLower();            
        }
    }
}