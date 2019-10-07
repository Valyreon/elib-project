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
using Microsoft.EntityFrameworkCore.Internal;
using System.Collections.Generic;

namespace ElibWpf.Database
{
    /// <summary>  
    ///  This class provides SQLite database interaction.  
    /// </summary>  
    public partial class DatabaseContext : DbContext
    {
        private DatabaseContext() :
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
}
}