using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using VersOne.Epub;
using VersOne.Epub.Options;

namespace Valyreon.Elib.EBookTools.Epub
{
    public class VersOneEpubParser : EbookParser
    {
        private readonly string filePath;

        private static readonly EpubReaderOptions epubReaderOptions = new()
        {
            ContentDownloaderOptions = new()
            {
                DownloadContent = false
            },
            Epub2NcxReaderOptions = new()
            {
                IgnoreMissingContentForNavigationPoints = true
            },
            PackageReaderOptions = new()
            {
                IgnoreMissingToc = true,
                SkipInvalidManifestItems = true
            },
            XmlReaderOptions = new()
            {
                SkipXmlHeaders = true
            }
        };

        public VersOneEpubParser(string filePath)
        {
            this.filePath = filePath;
        }

        public override string GenerateHtml()
        {
            throw new NotImplementedException();
        }

        public override ParsedBook Parse()
        {
            var epubBook = EpubReader.ReadBook(filePath, epubReaderOptions);

            var str = JsonSerializer.Serialize(epubBook.Schema, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(@"C:\Users\Luka\Desktop\log.txt", str);

            return new ParsedBook
            {
                Title = epubBook.Title.Clean(),
                Authors = epubBook.AuthorList.Select(a => a.Clean()).ToList(),
                Description = epubBook.Description.Clean(false, false).Prettify(3000),
                Cover = epubBook.CoverImage,
                Isbn = epubBook.Schema.Package?.Metadata?.Identifiers?.FirstOrDefault(i => i.Id?.ToLowerInvariant().Contains("isbn") == true || i.Scheme?.ToLowerInvariant()?.Contains("isbn") == true)?.Identifier?.Clean(),
                Publisher = epubBook.Schema.Package?.Metadata?.Publishers?.FirstOrDefault()?.Publisher?.Clean(),
                Path = filePath
            };
        }
    }
}
