using System;
using System.Collections.Generic;
using System.IO;
using Valyreon.Elib.EbookTools.Mobi;
using Valyreon.Elib.EBookTools.Epub;

namespace Valyreon.Elib.EBookTools
{
    public static class EbookParserFactory
    {
        public static IEnumerable<string> SupportedExtensions { get; } = new[] { ".epub", ".mobi", ".pdf" };

        public static EbookParser Create(string path)
        {
            return Path.GetExtension(path) switch
            {
                ".epub" => new VersOneEpubParser(path),
                ".mobi" => new MobiParser(path),
                ".pdf" => new PdfParser(path),
                _ => throw new ArgumentException($"{path} has an unkown extension.")
            };
        }
    }
}
