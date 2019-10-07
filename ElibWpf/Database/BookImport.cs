using EbookTools;
using EbookTools.Epub;
using EbookTools.Mobi;
using ElibWpf.DomainModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElibWpf.Database
{
    public class BookImport
    {
        private DatabaseContext database;
        private Book book;

        public BookImport(DatabaseContext database) => (this.database) = database;

        public ParsedBook GetParsedBook(string path)
        {
            ParsedBook newBook;
            if (System.IO.File.Exists(path))
            {
                StreamReader bookStream = new StreamReader(path);
                switch (Path.GetExtension(path))
                {
                    case ".mobi":
                        newBook = (new MobiParser(bookStream.BaseStream).Parse());
                        break;
                    case ".epub":
                        newBook = (new EpubParser(bookStream.BaseStream).Parse());
                        break;
                    default:
                        throw new FileFormatException($"File {path} has an unsupported extension.");
                }
            }
            else
                throw new FileNotFoundException($"File {path} does not exist.");

            return newBook;

        }

        public IList<ParsedBook> GetParsedBooks(IEnumerable<string> pathList)
        {
            IList<ParsedBook> result = new List<ParsedBook>();
            foreach (string path in pathList)
                result.Add(GetParsedBook(path));

            return result;
        }

        public IEnumerable<string> GetValidBookPaths(IEnumerable<string> pathList) 
            => pathList.Where(x => Path.GetExtension(x) == ".mobi" || Path.GetExtension(x) == ".epub");

        public Book CheckIfBookNameExists(ParsedBook parsedBook)
            => database.FindBookByName(parsedBook.Title);
    }
}
