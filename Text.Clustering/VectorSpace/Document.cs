using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Text.Clustering.Interface;
using Text.Clustering.TextUtils;

namespace Text.Clustering.VectorSpace
{
    public class Document : DocumentBase
    {
        protected ITextPreprocessor Proc = DefaultTextPreprocessor.Get;

        public ITextPreprocessor TextPreprocessor
        {
            get { return Proc; }
            set { Proc = value ?? DefaultTextPreprocessor.Get; }
        }

        protected override string FilterWord(string nextWord)
        {
            return Proc.Prepare(nextWord);
        }

        public Document(string source, ITextPreprocessor textPreprocessor) : base()
        {
            TextPreprocessor = textPreprocessor;
            PreProcessText(source);
        }

        public Document(string source) : base(source)
        {
        }

        public Document(Stream source, Encoding encoding) : base(source, encoding)
        {
        }
    }
}
