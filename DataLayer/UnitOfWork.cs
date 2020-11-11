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
		private IQuoteRepository quoteRepository;

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

		public IBookRepository BookRepository => bookRepository ??= new BookRepository(transaction);

		public ISeriesRepository SeriesRepository => seriesRepository ??= new SeriesRepository(transaction);

		public ICollectionRepository CollectionRepository => collectionRepository ??= new CollectionRepository(transaction);

		public IAuthorRepository AuthorRepository => authorRepository ??= new AuthorRepository(transaction);

		public IEFileRepository EFileRepository => eFileRepository ??= new EFileRepository(transaction);

		public IRawFileRepository RawFileRepository => rawFileRepository ??= new RawFileRepository(transaction);

		public ICoverRepository CoverRepository => coverRepository ??= new CoverRepository(transaction);

		public IQuoteRepository QuoteRepository => quoteRepository ??= new QuoteRepository(transaction);

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
			if(transaction != null)
			{
				transaction.Dispose();
				transaction = null;
			}
			if(connection != null)
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
			connection.Execute("DELETE FROM [AuthorBooks]");
			connection.Execute("DELETE FROM [Authors]");
			connection.Execute("DELETE FROM [EBookFiles]");
			connection.Execute("DELETE FROM [Covers]");
			connection.Execute("DELETE FROM [Books]");
			connection.Execute("DELETE FROM [Quotes]");
			connection.Execute("DELETE FROM [RawFiles]");
			connection.Execute("DELETE FROM [Series]");
			connection.Execute("DELETE FROM [UserCollectionBooks]");
			connection.Execute("DELETE FROM [UserCollections]");
		}
	}
}
