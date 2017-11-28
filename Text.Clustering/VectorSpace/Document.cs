using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;

namespace Text.Clustering.VectorSpace
{
    public class Document : DocumentBase
    {
        protected Regex ReplaceRegex = new Regex(@"[^a-zA-Zа-яА-Я0-9]", RegexOptions.Compiled | RegexOptions.CultureInvariant);

        protected static string alp = "abcdefghijklmopqrstuvwxyzABCDEFGHIJKLMOPQRSTUVWXYZ0123456789ёйфячыцувсмакепитрнгоьблшщдюжзхэъЁЙФЯЧЫЦУВСМАКЕПИТРНГОЬБЛШЩДЮЖЗХЭЪ";
        protected static HashSet<char> charset = new HashSet<char>(alp.Select(c=>c));
        protected StringBuilder sb = new StringBuilder();

        protected override string FilterWord(string nextWord)
        {
            //sb.Clear();
            //foreach (var ch in nextWord) if (charset.Contains(ch)) sb.Append(ch);
            //return sb.ToString().ToLower();
            return string.IsNullOrEmpty(nextWord) ? nextWord : ReplaceRegex.Replace(nextWord, "").ToLower();
        }

        public Document(string source) : base(source)
        {
        }

        public Document(Stream source, Encoding encoding) : base(source, encoding)
        {
        }
    }
}
