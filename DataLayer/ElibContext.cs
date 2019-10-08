using Domain;
using SQLite.CodeFirst;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Data.SQLite;
using System.Data.SQLite.EF6.Migrations;

namespace DataLayer
{
    public class ElibContext: DbContext
    {
        public DbSet<Book> Books { get; set; }
        public DbSet<Author> Authors { get; set; }
        public DbSet<EFile> BookFiles { get; set; }
        public DbSet<Quote> Quotes { get; set; }
        public DbSet<BookSeries> Series { get; set; }
        public DbSet<UserCollection> UserCollections { get; set; }

        static ElibContext()
        {
            Database.SetInitializer(new MigrateDatabaseToLatestVersion<ElibContext, Configuration>(true));
        }

        public ElibContext() :
            base(
                new SQLiteConnection()
                {
                    ConnectionString = new SQLiteConnectionStringBuilder()
                    {
                        DataSource = @"C:\Users\luka.budrak\Desktop\new_elib.db"
                    }
                    .ConnectionString
                },
                true)
        {
            DbConfiguration.SetConfiguration(new SQLiteConfiguration());
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Book>().HasMany(x => x.Authors);
            modelBuilder.Entity<Book>().HasMany(x => x.Files);
            modelBuilder.Entity<Book>().HasMany(x => x.Quotes);

            modelBuilder.Entity<AuthorBook>().HasKey(x => new { x.BookId, x.AuthorId});
            modelBuilder.Entity<CollectionBook>().HasKey(x => new { x.BookId, x.CollectionId });

            modelBuilder.Entity<Author>().HasMany(x => x.Books);
            modelBuilder.Entity<BookSeries>().HasMany(x => x.Books);

            var sqliteConnectionInitializer = new SqliteCreateDatabaseIfNotExists<ElibContext>(modelBuilder);
            Database.SetInitializer(sqliteConnectionInitializer);
        }
    }

    internal sealed class Configuration : DbMigrationsConfiguration<DataLayer.ElibContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
            AutomaticMigrationDataLossAllowed = true;
            SetSqlGenerator("System.Data.SQLite", new SQLiteMigrationSqlGenerator());
        }

        protected override void Seed(DataLayer.ElibContext context)
        {
            //  This method will be called after migrating to the latest version.

            //  You can use the DbSet<T>.AddOrUpdate() helper extension method
            //  to avoid creating duplicate seed data.
        }
    }
}
