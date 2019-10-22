using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain;
using EbookTools;
using Models.Utilities;

namespace Models.Helpers
{
    public static class ParsedBookHelper
    {
        public static EFile GetEFile(this ParsedBook parsedBook)
        {
            return new EFile()
            {
                Format = parsedBook.Format,
                RawContent = parsedBook.RawData
            };
        }

        public static Author GetAuthor(this ParsedBook parsedBook)
        {
            return DomainInitializer.NamedAuthor(parsedBook.Author);
        }

        public static Book GetBook(this ParsedBook parsedBook)
        {
            ImageConverter imageConverter = new ImageConverter();

            Book result = DomainInitializer.EmptyBook();

            result.Name = parsedBook.Title;
            result.Cover = (byte[])imageConverter.ConvertTo(parsedBook.Cover, typeof(byte[]));

            return result;
        }
    }
}
