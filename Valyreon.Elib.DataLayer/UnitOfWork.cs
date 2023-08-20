using System;
using System.Data;
using Dapper;
using Microsoft.Data.Sqlite;
using Valyreon.Elib.DataLayer.Interfaces;
using Valyreon.Elib.DataLayer.Repositories;

namespace Valyreon.Elib.DataLayer
{
    public class UnitOfWork : IUnitOfWork, IDisposable
    {
        private IDbConnection connection;
        private IDbTransaction transaction;

        private IBookRepository bookRepository;
        private IAuthorRepository authorRepository;
        private ICollectionRepository collectionRepository;
        private ISeriesRepository seriesRepository;
        private ICoverRepository coverRepository;
        private IQuoteRepository quoteRepository;

        internal UnitOfWork(string dbPath)
        {
            var connectionString = new SqliteConnectionStringBuilder
            {
                DataSource = dbPath,
                ForeignKeys = true
            }.ConnectionString;

            connection = new SqliteConnection(connectionString);
            connection.Open();
            transaction = connection.BeginTransaction();
        }

        public IBookRepository BookRepository => bookRepository ??= new BookRepository(transaction);

        public ISeriesRepository SeriesRepository => seriesRepository ??= new SeriesRepository(transaction);

        public ICollectionRepository CollectionRepository => collectionRepository ??= new CollectionRepository(transaction);

        public IAuthorRepository AuthorRepository => authorRepository ??= new AuthorRepository(transaction);

        public ICoverRepository CoverRepository => coverRepository ??= new CoverRepository(transaction);

        public IQuoteRepository QuoteRepository => quoteRepository ??= new QuoteRepository(transaction);

        public void Commit()
        {
            try
            {
                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
            finally
            {
                transaction.Dispose();
                transaction = connection.BeginTransaction();
                ResetRepositories();
            }
        }

        private void ResetRepositories()
        {
            bookRepository = null;
            authorRepository = null;
            collectionRepository = null;
            seriesRepository = null;
            coverRepository = null;
            quoteRepository = null;
        }

        public void Dispose()
        {
            if (transaction != null)
            {
                transaction.Dispose();
                transaction = null;
            }
            if (connection != null)
            {
                connection.Dispose();
                connection = null;
            }
            GC.SuppressFinalize(this);
            UnitOfWorkFactory.ReleaseSemaphore();
        }

        private static void ClearCache()
        {
            new BookRepository(null).ClearCache();
            new SeriesRepository(null).ClearCache();
            new AuthorRepository(null).ClearCache();
            new CollectionRepository(null).ClearCache();
            new QuoteRepository(null).ClearCache();
        }

        public void Truncate()
        {
            connection.Execute("DELETE FROM [AuthorBooks]");
            connection.Execute("DELETE FROM [Authors]");
            connection.Execute("DELETE FROM [Books]");
            connection.Execute("DELETE FROM [Covers]");
            connection.Execute("DELETE FROM [EBookFiles]");
            connection.Execute("DELETE FROM [Quotes]");
            connection.Execute("DELETE FROM [RawFiles]");
            connection.Execute("DELETE FROM [Series]");
            connection.Execute("DELETE FROM [UserCollectionBooks]");
            connection.Execute("DELETE FROM [UserCollections]");
        }

        public void Vacuum()
        {
            transaction.Dispose();
            /* It is important to note that the VACCUM command requires storage to hold the original file
             * and also the copy. Also, the VACUUM command requires exclusive access to the database file.
             * In other words, the VACUUM command will not run successfully if the database has a pending
             * SQL statement or an open transaction. */
            connection.Execute("VACUUM;");
            transaction = connection.BeginTransaction();
        }
    }
}
