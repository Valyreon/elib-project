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

        public DbSet<Autor> Autor { get; set; }
        public DbSet<Citat> Citat { get; set; }
        public DbSet<Fajl> Fajl { get; set; }
        public DbSet<Knjiga> Knjiga { get; set; }
        public DbSet<Kolekcija> Kolekcija { get; set; }
        public DbSet<Serijal> Serijal { get; set; }
        public DbSet<knjiga_autor> knjiga_autor { get; set; }
        public DbSet<kolekcija_knjiga> kolekcija_knjiga { get; set; }

        public void AddBookDB(String name)
        {
            Knjiga.Add(new Knjiga
            {
                naziv = name.Trim(),
            });
            this.SaveChangesAsync();
        }

        public Knjiga AddBookDB(Knjiga book)
        {
            Knjiga result = Knjiga.Add(book);
            this.SaveChangesAsync();
            return result;
        }

        public Autor AddAuthorDB(Autor author)
        {
            Autor result = Autor.Add(author);
            this.SaveChangesAsync();
            return result;
        }

        public void ListAllBooks()
        {
            foreach(var x in Knjiga)
            {
                Console.WriteLine(x.id + " : " + x.naziv);
            }
        }

        public void ListAllAuthors()
        {
            foreach(var x in Autor)
            {
                Console.WriteLine(x.ime);
            }
        }

        public knjiga_autor AddBookAuthorLink(Knjiga book, Autor autor)
        {
            knjiga_autor newBookAuthorLink = new knjiga_autor
            {
                knjiga_id = book.id,
                autor_id = autor.id
            };
            knjiga_autor result = knjiga_autor.Add(newBookAuthorLink);
            this.SaveChangesAsync();
            return result;
        }

        public void Info()
        {
            
        }

        

        public void ImportBook(string path)
        {
            if (File.Exists(path))
            {
                byte[] fileBinary = File.ReadAllBytes(path);
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
                Knjiga newBook = Knjiga.Add(parsedBook.GetBook());//Add parsed book data to book table

                //Check if author exists in table
                Autor tempAuthor = FindAuthor(parsedBook.Author);
                if (tempAuthor == null)
                {
                    //Add to author table
                    Autor newAuthor = new Autor
                    {
                        ime = parsedBook.Author
                    };
                    Autor.Add(newAuthor);
                    //Add to link table
                    knjiga_autor newBookAuthorLink = AddBookAuthorLink(newBook, newAuthor);
                    newAuthor.knjiga_autorValues.Add(newBookAuthorLink);
                }
                else
                {
                    //Update the link table
                    knjiga_autor newBookAuthorLink = AddBookAuthorLink(newBook, tempAuthor);
                    tempAuthor.knjiga_autorValues.Add(newBookAuthorLink); 
                }
                //Add file to the file table
                Fajl.Add(new Fajl
                {
                    knjigaId = newBook.id,
                    fajl = fileBinary,
                    format = Path.GetExtension(path)
                });
                this.SaveChanges();
                Console.WriteLine($"Successfully added {parsedBook.Author} - {parsedBook.Title}");
            }
            else
                Console.WriteLine("System cannot load file " + path);
        }

        public Knjiga GetBookFromID(long id) => Knjiga.Find(id);
        public Autor FindAuthor(string author) => Autor.FirstOrDefault(x => x.ime == author);

        public void BookMetadata(long id)
        {
            Knjiga book = GetBookFromID(id);
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