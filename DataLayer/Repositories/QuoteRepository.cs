using Dapper;
using Domain;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace DataLayer.Repositories
{
	public class QuoteRepository : RepositoryBase, IQuoteRepository
	{
		public QuoteRepository(IDbTransaction transaction) : base(transaction)
		{
		}

		public void Add(Quote entity)
		{
			entity.Id = Connection.ExecuteScalar<int>(
				"INSERT INTO Quotes(Text, BookId, Note) VALUES (@Text, @BookId, @Note); SELECT last_insert_rowid() ",
				entity,
				Transaction
			);
		}

		public IEnumerable<Quote> All()
		{
			return Connection.Query<Quote>("SELECT * FROM Quotes", Transaction);
		}

		public Quote Find(int id)
		{
			return Connection.Query<Quote>("SELECT * FROM Quotes WHERE Id = @QuoteId LIMIT 1", new { QuoteId = id }, Transaction).FirstOrDefault();
		}

		public IEnumerable<Quote> GetQuotesFromBook(int bookId)
		{
			return Connection.Query<Quote>("SELECT * FROM Quotes WHERE BookId = @BookId LIMIT 1", new { BookId = bookId }, Transaction);
		}

		public void Remove(int id)
		{
			Connection.Execute("DELETE FROM Quotes WHERE Id = @RemoveId", new { RemoveId = id }, Transaction);
		}

		public void Remove(Quote entity)
		{
			Remove(entity.Id);
			entity.Id = 0;
		}

		public void Update(Quote entity)
		{
			Connection.Execute("UPDATE Quotes SET Text = @Text, Note = @Note, BookId = @BookId WHERE Id = @Id", entity, Transaction);
		}
	}
}
