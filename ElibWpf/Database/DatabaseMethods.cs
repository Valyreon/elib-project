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
        public void AddBookDB(String name)
        {
            Books.Add(new Book
            {
                name = name.Trim(),
            });
            this.SaveChanges();
        }

        /// <summary>  
        ///  Adds the book passed as the argument to the local database.
        ///  <param name="book">Book object</param>
        ///  <returns>The Book object with filled id property</returns>
        /// </summary>
        public Book AddBookDB(Book book)
        {
            Book result = Books.Add(book);
            this.SaveChanges();
            return result;
        }

        public Series AddSeriesDB(Series series)
        {
            Series result = Series.Add(series);
            this.SaveChanges();
            return result;
        }

        public DomainModel.File AddFileDB(DomainModel.File file)
        {
            DomainModel.File result = Files.Add(file);
            this.SaveChanges();
            return result;
        }

        /// <summary>  
        ///  Adds the author passed as the argument to the local database.
        ///  <param name="author">Author object</param>
        ///  <returns>The Author object with filled id property</returns>
        /// </summary>
        public Author AddAuthorDB(Author author)
        {
            Author result = Authors.Add(author);
            this.SaveChanges();
            return result;
        }

        /// <summary>  
        ///  Adds the passed Author as the author of the passed Book to the local database.
        ///  <param name="book">Book object</param>
        ///  <param name="author">Author object</param>
        ///  <returns>The book_author object linking the passed Book and Authors</returns>
        /// </summary>
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
            this.SaveChangesAsync();
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
        public Series GetSeriesFromID(long id) => Series.Find(id);
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
            this.SaveChanges();
            return GetBookFromID(book.id);
        }

        public Series AddOrUpdateSeries(Series series)
        {
            Series.AddOrUpdate(series);
            this.SaveChanges();
            return series;
        }
    }
}
