using EbookTools;
using ElibWpf.DomainModel;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElibWpf.Database
{
    public static class Extensions
    {
        public static Book GetBook(this ParsedBook parsedBook)
        {
            //Convert Image to bytearray to keep in the database
            byte[] coverBinary;
            using (var ms = new MemoryStream())
            {
                parsedBook.Cover.Save(ms, parsedBook.Cover.RawFormat);
                coverBinary = ms.ToArray();
            }
            //construct JSON metadata
            var bookMetadata = new MetadataModels.BookMetadata
            {
                ISBN = parsedBook.ISBN,
                Publisher = parsedBook.Publisher
            };



            return new Book
            {
                name = parsedBook.Title,
                cover = coverBinary,
                metadata = JsonConvert.SerializeObject(bookMetadata)
            };
        }
    }
}
