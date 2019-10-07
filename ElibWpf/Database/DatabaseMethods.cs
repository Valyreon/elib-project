using EbookTools;
using EbookTools.Epub;
using EbookTools.Mobi;
using ElibWpf.Database.MetadataModels;
using ElibWpf.DomainModel;
using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElibWpf.Database
{
    /// <summary>  
    ///  This partial class provides methods for database interaction.  
    /// </summary> 
    public partial class DatabaseContext
    {
        public Book AddBookDB(Book book) => Books.Add(book);
        public Series AddSeriesDB(Series series) => Series.Add(series);
        public DomainModel.File AddFileDB(DomainModel.File file) => Files.Add(file);
        public Author AddAuthorDB(Author author) => Authors.Add(author);
        public book_author AddBookAuthorLink(Book book, Author author)
        {
            book_author newBookAuthorLink = new book_author
            {
                book_id = book.id,
                author_id = author.id,
                book = book,
                author = author
            };
            book_author result = book_author.Add(newBookAuthorLink);
            return result;
        }


        public IList<Author> GetBookAuthors(Book book)
            => book_author.Where(x => x.book_id == book.id).Select(i => i.author).ToList();
        public IList<Book> GetAuthorBooks(Author author)
           => book_author.Where(x => x.author_id == author.id).Select(i => i.book).ToList();
        public IList<Collection> GetBookCollections(Book book)
            => collection_book.Where(x => x.book_id == book.id).Select(i => i.collection).ToList();


        /// <summary>  
        ///  Imports the book at the specified path to the local database.
        ///  <param name="path">Path to the book on the filesystem</param>
        /// </summary>
        public void ImportBook(string path)
        {
            if (System.IO.File.Exists(path))
            {
                byte[] fileBinary = System.IO.File.ReadAllBytes(path);
                EbookParser ebookParser;
                switch (Path.GetExtension(path).ToLower())
                {
                    case ".epub":
                        ebookParser = new EpubParser(fileBinary);
                        break;
                    case ".mobi":
                        ebookParser = new MobiParser(fileBinary);
                        break;
                    default:
                        Console.WriteLine("Unknown file format");
                        return;

                }
                ParsedBook parsedBook = ebookParser.Parse();
                Book newBook = Books.Add(parsedBook.GetBook());//Add parsed book data to book table

                //Check if author exists in table
                Author tempAuthor = FindAuthor(parsedBook.Author);
                if (tempAuthor == null)
                {
                    //Add to author table
                    Author newAuthor = new Author
                    {
                        name = parsedBook.Author
                    };
                    Authors.Add(newAuthor);
                    //Add to link table
                    book_author newBookAuthorLink = AddBookAuthorLink(newBook, newAuthor);
                    newAuthor.book_authorValues.Add(newBookAuthorLink);
                }
                else
                {
                    //Update the link table
                    book_author newBookAuthorLink = AddBookAuthorLink(newBook, tempAuthor);
                    tempAuthor.book_authorValues.Add(newBookAuthorLink);
                }
                //Add file to the file table
                Files.Add(new DomainModel.File
                {
                    bookId = newBook.id,
                    fileBlob = fileBinary,
                    format = Path.GetExtension(path)
                });
                this.SaveChanges();
                Console.WriteLine($"Successfully added {parsedBook.Author} - {parsedBook.Title}");
            }
            else
                Console.WriteLine("System cannot load file " + path);
        }

        public Book GetBookFromID(long id) => Books.Find(id);
        public Collection GetCollectionFromID(long id) => Collections.Find(id);
        public Author GetAuthorFromID(long id) => Authors.Find(id);
        public Series GetSeriesFromID(long id) => Series.Find(id);
        public Book FindBookByName(string name) => Books.Where(x => x.name == name).FirstOrDefault();
        public Author FindAuthor(string author) => Authors.FirstOrDefault(x => x.name == author);
        public Series FindSeries(string series) => Series.FirstOrDefault(x => x.name == series);
        public IList<Book> FindBooks(string bookName) => Books.Where(x => x.name.ToLower().Contains(bookName)).ToList();
        public IList<Author> FindAuthors(string authorName) => Authors.Where(x => x.name.ToLower().Contains(authorName)).ToList();

        public BookMetadata GetBookMetadata(long id)
        {
            Book book = GetBookFromID(id);
            if (book != null)
                return BookMetadata.GetBookMetadataFromJson(book.metadata);
            else
                return null;
        }

        private static DatabaseContext instance = null;

        public static DatabaseContext GetInstance()
        {
            if (instance == null)
                instance = new DatabaseContext();
            return instance;
        }

        public Book AddOrUpdateBook(Book book)
        {
            Books.AddOrUpdate(book);
            return GetBookFromID(book.id);
        }

        public Series AddOrUpdateSeries(Series series)
        {
            Series.AddOrUpdate(series);
            return GetSeriesFromID(series.id);
        }

        public Author AddOrUpdateAuthor(Author author)
        {
            Authors.AddOrUpdate(author);
            return GetAuthorFromID(author.id);
        }
    }
}
