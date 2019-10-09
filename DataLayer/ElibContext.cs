using Domain;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Data.SQLite;

namespace DataLayer
{
    public class ElibContext : DbContext
    {
        public DbSet<Book> Books { get; set; }
        public DbSet<Author> Authors { get; set; }
        public DbSet<EFile> BookFiles { get; set; }
        public DbSet<Quote> Quotes { get; set; }
        public DbSet<BookSeries> Series { get; set; }
        public DbSet<UserCollection> UserCollections { get; set; }

        public ElibContext(string source) :
            base(
                new SQLiteConnection()
                {
                    ConnectionString = new SQLiteConnectionStringBuilder()
                    {
                        DataSource = source
                    }
                    .ConnectionString
                },
                true)
        {
            DbConfiguration.SetConfiguration(new SQLiteConfiguration());
            Database.ExecuteSqlCommand("PRAGMA foreign_keys = ON");
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            // Here we do schema building with Fluent API
            modelBuilder.Entity<Book>().HasMany(x => x.Authors);
            modelBuilder.Entity<Book>().HasMany(x => x.Files);
            modelBuilder.Entity<Book>().HasMany(x => x.Quotes);

            modelBuilder.Entity<AuthorBookLink>().HasKey(x => new { x.BookId, x.AuthorId });
            modelBuilder.Entity<CollectionBookLink>().HasKey(x => new { x.BookId, x.CollectionId });

            modelBuilder.Entity<Author>().HasMany(x => x.Books);
            modelBuilder.Entity<BookSeries>().HasMany(x => x.Books);

            modelBuilder.Entity<UserCollection>().HasIndex(x => x.Tag).IsUnique();
        }
    }
}
