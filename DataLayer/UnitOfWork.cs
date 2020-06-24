using Dapper;
using DataLayer.Repositories;
using System.Data;
using System.Data.SQLite;

namespace DataLayer
{
    public class UnitOfWork : IUnitOfWork
    {
        private IDbConnection connection;
        private IDbTransaction transaction;

        private IBookRepository bookRepository;
        private IAuthorRepository authorRepository;
        private ICollectionRepository collectionRepository;
        private ISeriesRepository seriesRepository;
        private IEFileRepository eFileRepository;
        private IRawFileRepository rawFileRepository;
        private ICoverRepository coverRepository;

        public UnitOfWork(string dbPath)
        {
            var connectionString = new SQLiteConnectionStringBuilder
            {
                DataSource = dbPath
            }.ConnectionString;

            connection = new SQLiteConnection(connectionString);
            connection.Open();
            transaction = connection.BeginTransaction();
        }

        public IBookRepository BookRepository { get { return bookRepository ?? (bookRepository = new BookRepository(transaction)); } }

        public ISeriesRepository SeriesRepository { get { return seriesRepository ?? (seriesRepository = new SeriesRepository(transaction)); } }

        public ICollectionRepository CollectionRepository { get { return collectionRepository ?? (collectionRepository = new CollectionRepository(transaction)); } }

        public IAuthorRepository AuthorRepository { get { return authorRepository ?? (authorRepository = new AuthorRepository(transaction)); } }

        public IEFileRepository EFileRepository { get { return eFileRepository ?? (eFileRepository = new EFileRepository(transaction)); } }

        public IRawFileRepository RawFileRepository { get { return rawFileRepository ?? (rawFileRepository = new RawFileRepository(transaction)); } }

        public ICoverRepository CoverRepository { get { return coverRepository ?? (coverRepository = new CoverRepository(transaction)); } }

        public void Commit()
        {
            //await semaphore.WaitAsync();
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
            //semaphore.Release();
        }

        private void ResetRepositories()
        {
            bookRepository = null;
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
        }

        public void ClearCache()
        {
            BookRepository.ClearCache();
        }

        public void Truncate()
        {
            this.connection.Execute("DELETE FROM [AuthorBooks]");
            this.connection.Execute("DELETE FROM [Authors]");
            this.connection.Execute("DELETE FROM [EBookFiles]");
            this.connection.Execute("DELETE FROM [Covers]");
            this.connection.Execute("DELETE FROM [Books]");
            this.connection.Execute("DELETE FROM [Quotes]");
            this.connection.Execute("DELETE FROM [RawFiles]");
            this.connection.Execute("DELETE FROM [Series]");
            this.connection.Execute("DELETE FROM [UserCollectionBooks]");
            this.connection.Execute("DELETE FROM [UserCollections]");
        }
    }
}