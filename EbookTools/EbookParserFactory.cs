using EbookTools.Epub;
using EbookTools.Mobi;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EbookTools
{
    public class EbookParserFactory
    {
        public static IEnumerable<string> SupportedExtensions { get; } = new string[] { ".epub", ".mobi" };

        public static EbookParser Create(string path)
        {
            switch(Path.GetExtension(path))
            {
                case ".epub":
                    return new EpubParser(File.OpenRead(path));
                case ".mobi":
                    return new MobiParser(File.OpenRead(path));
                default:
                    throw new ArgumentException($"{path} has an unkown extension.");
            }
        }
    }
}
