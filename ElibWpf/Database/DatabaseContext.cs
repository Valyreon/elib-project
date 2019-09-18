using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using System.Data.SQLite;
using ElibWpf.DomainModel;

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
            this.SaveChanges();
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

    }
}