﻿using System;
using System.Collections.Generic;
using System.IO;
using EbookTools.Epub;
using EbookTools.Mobi;

namespace EbookTools
{
    public class EbookParserFactory
    {
        public static IEnumerable<string> SupportedExtensions { get; } = new[] {".epub", ".mobi"};

        public static EbookParser Create(string path)
        {
            return Path.GetExtension(path) switch
            {
                ".epub" => new EpubParser(File.ReadAllBytes(path)),
                ".mobi" => new MobiParser(File.ReadAllBytes(path)),
                _ => throw new ArgumentException($"{path} has an unkown extension.")
            };
        }
    }
}