using System;
using System.Data.Entity;
using System.Data.SQLite;
using Domain;

namespace DataLayer
{
    public class ElibContext : DbContext
    {
        public ElibContext(string source) :
            base(
                new SQLiteConnection
                {
                    ConnectionString = new SQLiteConnectionStringBuilder
                        {
                            DataSource = source
                        }
                        .ConnectionString
                },
                true)
        {
            DbConfiguration.SetConfiguration(new SQLiteConfiguration());
            this.Configuration.LazyLoadingEnabled = false;
            this.Database.ExecuteSqlCommand("PRAGMA foreign_keys = ON");
            this.Database.Log = Console.WriteLine;
        }

        public DbSet<Author> Authors { get; set; }
        public DbSet<EFile> BookFiles { get; set; }

        public DbSet<Book> Books { get; set; }
        public DbSet<Quote> Quotes { get; set; }
        public DbSet<BookSeries> Series { get; set; }
        public DbSet<UserCollection> UserCollections { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            // Here we do schema building with Fluent API
            modelBuilder.Entity<Book>().HasMany(x => x.Authors);
            //modelBuilder.Entity<Book>().Has(x => x.Files);
            modelBuilder.Entity<Book>().HasMany(x => x.Quotes);

            modelBuilder.Entity<AuthorBookLink>().HasKey(x => new {x.BookId, x.AuthorId});
            modelBuilder.Entity<CollectionBookLink>().HasKey(x => new {x.BookId, x.UserCollectionId});

            /*modelBuilder.Entity<Author>().HasMany(x => x.Books);
            modelBuilder.Entity<BookSeries>().HasMany(x => x.Books);*/

            modelBuilder.Entity<UserCollection>().HasIndex(x => x.Tag).IsUnique();
        }

        public void Vacuum()
        {
            this.Database.ExecuteSqlCommand(TransactionalBehavior.DoNotEnsureTransaction,
                "VACUUM;"); // this solves the db size problem
        }

        public void TruncateDatabase()
        {
            this.Database.ExecuteSqlCommand("DELETE FROM [AuthorBooks]");
            this.Database.ExecuteSqlCommand("DELETE FROM [Authors]");
            this.Database.ExecuteSqlCommand("DELETE FROM [EBookFiles]");
            this.Database.ExecuteSqlCommand("DELETE FROM [Books]");
            this.Database.ExecuteSqlCommand("DELETE FROM [Quotes]");
            this.Database.ExecuteSqlCommand("DELETE FROM [RawFiles]");
            this.Database.ExecuteSqlCommand("DELETE FROM [Series]");
            this.Database.ExecuteSqlCommand("DELETE FROM [UserCollectionBooks]");
            this.Database.ExecuteSqlCommand("DELETE FROM [UserCollections]");
            this.Vacuum();
        }
    }
}