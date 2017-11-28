using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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

        static void Main(string[] args)
        {
            var enc = Encoding.GetEncoding(1251);
            var files = Directory.GetFiles(Directory.GetCurrentDirectory(), "*.txt");
            var list = new List<Document>();

            Dictionary<BookInfo,Document> docs = new Dictionary<BookInfo, Document>();

            foreach (var file in files)
            {
                using (var rdr = new StreamReader(file, enc))
                {
                    while (!rdr.EndOfStream)
                    {
                        var bookInfo = new BookInfo(rdr);
                        var text = bookInfo.Descr;
                        if (text == null || text.Contains("PDF")) text = bookInfo.Annot;
                        if (text == null || text.Contains("PDF")) text = bookInfo.Title;
                        if (string.IsNullOrEmpty(text)) continue;
                        var document = new Document(text);
                        list.Add(document);
                        docs[bookInfo] = document;
                    }
                }
            }
            var corpus = new Corpus(list);
            var model = new VectorSpaceModel(corpus);
            double[,] dist = new double[list.Count,list.Count];
            for (int i = 0; i < list.Count; i++)
            {
                for (int j = i; j < list.Count; j++)
                {
                    var d = model.CosineSimilarity(list[i], list[j]);
                    dist[i, j] = d;
                    dist[j, i] = d;
                }
            }
            Console.ReadLine();
        }
    }
}
