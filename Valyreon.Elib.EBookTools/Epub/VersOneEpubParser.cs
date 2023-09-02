using System;
using System.Collections.Generic;
using System.Linq;
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

            var authorList = new List<string>();

            foreach (var parsedAuthor in epubBook.AuthorList)
            {
                if (parsedAuthor.Contains(" and "))
                {
                    var resultAuthors = parsedAuthor.Split(" and ");
                    authorList.AddRange(resultAuthors);
                }
                else
                {
                    authorList.Add(parsedAuthor);
                }
            }

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
