using System;
using System.Data.Entity;
using System.Data.SQLite;
using ElibWpf.DomainModel;
using System.IO;
using EbookTools.Epub;
using EbookTools;
using System.Drawing;
using Newtonsoft.Json;
using EbookTools.Mobi;
using System.Linq;

namespace ElibWpf.Database
{
    public class DatabaseContext : DbContext
    {
        public DatabaseContext() :
            base(
                new SQLiteConnection()
                {
                    ConnectionString = new SQLiteConnectionStringBuilder()
                    {
                        DataSource = "./db.sqlite"
                    }
                    .ConnectionString
                }, true)
        {
            System.Data.Entity.Database.SetInitializer<DatabaseContext>(null);
            Database.ExecuteSqlCommand("PRAGMA foreign_keys = ON");
            DbConfiguration.SetConfiguration(new SQLiteConfig());
        }

        public DbSet<Author> Authors { get; set; }
        public DbSet<Quote> Quotes { get; set; }
        public DbSet<DomainModel.File> Files { get; set; }
        public DbSet<Book> Books { get; set; }
        public DbSet<Collection> Collections { get; set; }
        public DbSet<Series> Series { get; set; }
        public DbSet<book_author> book_author { get; set; }
        public DbSet<collection_book> collection_book { get; set; }

        public void AddBookDB(String name)
        {
            Books.Add(new Book
            {
                name = name.Trim(),
            });
            this.SaveChangesAsync();
        }

        public Book AddBookDB(Book book)
        {
            Book result = Books.Add(book);
            this.SaveChangesAsync();
            return result;
        }

        public Author AddAuthorDB(Author author)
        {
            Author result = Authors.Add(author);
            this.SaveChangesAsync();
            return result;
        }

        public void ListAllBooks()
        {
            foreach(var x in Books)
            {
                Console.WriteLine(x.id + " : " + x.name);
            }
        }

        public void ListAllAuthors()
        {
            foreach(var x in Authors)
            {
                Console.WriteLine(x.name);
            }
        }

        public book_author AddBookAuthorLink(Book book, Author author)
        {
            book_author newBookAuthorLink = new book_author
            {
                book_id = book.id,
                author_id = author.id
            };
            book_author result = book_author.Add(newBookAuthorLink);
            this.SaveChangesAsync();
            return result;
        }

        public void Info()
        {
            
        }

        

        public void ImportBook(string path)
        {
            if (System.IO.File.Exists(path))
            {
                byte[] fileBinary = System.IO.File.ReadAllBytes(path);
                EbookParser ebookParser;
                switch(Path.GetExtension(path).ToLower())
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
        public Author FindAuthor(string author) => Authors.FirstOrDefault(x => x.name == author);

        public void BookMetadata(long id)
        {
            Book book = GetBookFromID(id);
            if(book != null)
            {
                Console.WriteLine(book.metadata);
            }
            else
            {
                Console.WriteLine("Book was not found");
            }
        }
}
}