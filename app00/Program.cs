using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NHunspell;
using Text.Clustering.Hashing;
using Text.Clustering.Interface;
using Text.Clustering.TextUtils;
using Text.Clustering.VectorSpace;

namespace app00
{
    class Program
    {
        public class BookInfo
        {
            public int Year { get; set; }
            
            public string Title { get; set; }
            public string Annot { get; set; }
            public string Descr { get; set; }

            public string BookUrl { get; set; }
            public string PdfUrl { get; set; }

            public string ProcessingText { get; set; }
            public List<string> KeyWords { get; set; }

            public BookInfo(StreamReader rdr)
            {
                while (!rdr.EndOfStream)
                {
                    var ws = rdr.ReadLine() ?? string.Empty;
                    if (string.IsNullOrEmpty(ws)) continue;
                    if (ws.StartsWith("-------")) break;
                    var pp = ws.Split(new string[] { "::" }, StringSplitOptions.RemoveEmptyEntries);
                    if (pp.Length < 2) continue;
                    if (pp[0] == "year") Year = int.Parse(pp[1]);
                    if (pp[0] == "book_url") BookUrl = pp[1];
                    if (pp[0] == "title") Title = pp[1];
                    if (pp[0] == "annot") Annot = pp[1];
                    if (pp[0] == "descr") Descr = pp[1];
                    if (pp[0] == "pdf_url") PdfUrl = pp[1];
                }
            }
        }

        private static Hunspell spell = new Hunspell("russian-aot.aff", "russian-aot.dic");
        
        public class SpellProcessor : ITextPreprocessor
        {
            public string Prepare(string src)
            {
                var ws = DefaultTextPreprocessor.Get.Prepare(src);

                var words = spell.Stem(ws);
                if (words != null) words.Sort((a, b) =>
                {
                    var ret = a.Length.CompareTo(b.Length);
                    if (ret == 0) ret = a.CompareTo(b);
                    return ret;
                });

                return words == null || words.Count == 0 ? ws : words[0];
            }
        }

        static void Main(string[] args)
        {
            var enc = Encoding.GetEncoding(1251);
            var files = Directory.GetFiles(Directory.GetCurrentDirectory(), "*.txt");

            var list = new List<Document>();
            var books = new List<BookInfo>();

            Dictionary<BookInfo,Document> book2doc = new Dictionary<BookInfo, Document>();
            Dictionary<DocumentBase,BookInfo> doc2book = new Dictionary<DocumentBase, BookInfo>();

            var spellProc = new SpellProcessor();

            foreach (var file in files)
            {
                Console.WriteLine("processing: {0}...", Path.GetFileName(file));
                using (var rdr = new StreamReader(file, enc))
                {
                    while (!rdr.EndOfStream)
                    {
                        var bookInfo = new BookInfo(rdr);
                        var text = bookInfo.Descr;
                        if (text == null || text.Contains("PDF")) text = bookInfo.Annot;
                        if (text == null || text.Contains("PDF")) text = bookInfo.Title;
                        if (string.IsNullOrEmpty(text)) continue;
                        //
                        bookInfo.ProcessingText = text.ToLower();
                        var document = new Document(text, spellProc);
                        book2doc[bookInfo] = document;
                        doc2book[document] = bookInfo;
                        //
                        books.Add(bookInfo);
                        list.Add(document);
                    }
                }
            }
            var corpus = new Corpus(list);
            var model = new VectorSpaceModel(corpus);

            string[] skipList = new[] { "о", "общие", "сведение", "даваться", "по", "для", "профиль", "из", "на", 
                "при", "вуз", "даны", "их", "предназначить", "студент", "подготовка", "бакалавр", "обучение",
                "всех", "направление", "обучаться" };
            string[] reqList = new[] { "учебный", "пособие", "методический", "курсовой", "дипломный", "работа", 
                "проект", "лабораторный" };

            Dictionary<string,List<BookInfo>> keys2book = new Dictionary<string, List<BookInfo>>();

            foreach (var document in list)
            {
                var ww = model.GetWeights(document);
                var pairs = ww.Where(x=>x.Value > 0).Select(x => x).ToList();
                pairs.Sort((a,b)=>b.Value.CompareTo(a.Value));
                //
                var book = doc2book[document];
                book.KeyWords = new List<string>();
                foreach (var pair in pairs)
                {
                    var needInclude = book.KeyWords.Count < 10;
                    var key = pair.Key;
                    if (needInclude) needInclude = (Array.IndexOf(skipList, key) == -1) && (key.Length > 1);
                    if (!needInclude) needInclude = Array.IndexOf(reqList, key) != -1;
                    if (!needInclude) continue;
                    book.KeyWords.Add(key);
                    if (!keys2book.ContainsKey(key)) keys2book[key] = new List<BookInfo>();
                    keys2book[key].Add(book);
                }
            }

            ////var d = model.CosineSimilarity(list[0], list[1]);
            //var lsh = new LocalitySensitiveHashing(0.1, 0.5, 0.1, 0.8);
            //lsh.Hashing.Extractor = new NgramExtractor(3);
            //for (int index = 0; index < books.Count; index++) lsh.Process(index, books[index].ProcessingText);
            //for (int index = 0; index < books.Count; index++)
            //{
            //    var indexes = lsh.FindSimiliar(books[index].ProcessingText);
            //    Console.WriteLine("src: {0}", books[index].ProcessingText);
            //    //foreach (var idx in indexes)
            //    //{
            //    //    if (idx == index) continue;
            //    //    var dist = model.CosineSimilarity(list[index], list[idx]);
            //    //    if (dist <= 1e-2) continue;
            //    //    Console.WriteLine();
            //    //    Console.WriteLine("dst: {0} => {1}", books[idx].ProcessingText, dist);
            //    //}

            //    var dists = indexes.Select(idx => model.CosineSimilarity(list[index], list[idx])).ToArray();

            //    Console.WriteLine("------------------------------------------------------------------------------------");
            //}
            
            ////double[,] dist = new double[list.Count,list.Count];
            ////for (int i = 0; i < list.Count; i++)
            ////{
            ////    for (int j = i; j < list.Count; j++)
            ////    {
            ////        var d = model.CosineSimilarity(list[i], list[j]);
            ////        dist[i, j] = d;
            ////        dist[j, i] = d;
            ////    }
            ////}

            var wordPairs = keys2book.Select(x => x).ToList();
            wordPairs.Sort((a,b)=>b.Value.Count.CompareTo(a.Value.Count));

            using (var wrt = new StreamWriter("keys.lst"))
            {
                foreach (var wp in wordPairs) wrt.WriteLine("{0} => {1}", wp.Key, wp.Value.Count);
                wrt.Flush();
            }

            
            //Console.ReadLine();
        }
    }
}
