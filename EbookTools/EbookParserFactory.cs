using EbookTools.Epub;
using EbookTools.Mobi;
using System;
using System.Collections.Generic;
using System.IO;

namespace EbookTools
{
    public class EbookParserFactory
    {
        public static IEnumerable<string> SupportedExtensions { get; } = new string[] { ".epub", ".mobi" };

        public static EbookParser Create(string path) =>  
            (Path.GetExtension(path)) switch
            {
                ".epub" => new EpubParser(File.ReadAllBytes(path)),
                ".mobi" => new MobiParser(File.ReadAllBytes(path)),
                _ => throw new ArgumentException($"{path} has an unkown extension."),
            };
    }
}