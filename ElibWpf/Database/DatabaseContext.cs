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

        public void AddBook(String name)
        {
            Knjiga.Add(new Knjiga
            {
                naziv = name.Trim(),
            });
            this.SaveChangesAsync();
        }

        public void AddBook(Knjiga book)
        {
            Knjiga.Add(book);
            this.SaveChangesAsync();
        }

        public void ListAllBooks()
        {
            foreach(var x in Knjiga)
            {
                Console.WriteLine(x.id + " : " + x.naziv);
            }
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
                AddBook(parsedBook.GetBook());
                Fajl.Add(new Fajl
                {
                    fajl = fileBinary,
                    format = Path.GetExtension(path)
                });
                Console.WriteLine($"Successfully added {parsedBook.Author} - {parsedBook.Title}");
            }
            else
                Console.WriteLine("System cannot load file " + path);
        }

        public Knjiga GetBookFromID(long id) => Knjiga.Find(id);

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