using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using DataLayer.Extensions;
using DataLayer.Interfaces;
using Domain;

namespace DataLayer.Repositories
{
    public class QuoteRepository : RepositoryBase, IQuoteRepository
    {
        public IDictionary<int, Quote> cache = new Dictionary<int, Quote>();

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
            cache.Add(entity.Id, entity);
        }

        public async Task AddAsync(Quote entity)
        {
            entity.Id = await Connection.ExecuteScalarAsync<int>(
                "INSERT INTO Quotes(Text, BookId, Note) VALUES (@Text, @BookId, @Note); SELECT last_insert_rowid() ",
                entity,
                Transaction
            );
            cache.Add(entity.Id, entity);
        }

        public IEnumerable<Quote> All()
        {
            var allList = Connection.Query<Quote>("SELECT * FROM Quotes", Transaction);

            foreach (var quote in allList.Where(x => !cache.ContainsKey(x.Id)))
            {
                cache.Add(quote.Id, quote);
            }

            return cache.Values.ToList();
        }

        public async Task<IEnumerable<Quote>> AllAsync()
        {
            var allList = await Connection.QueryAsync<Quote>("SELECT * FROM Quotes", Transaction);

            foreach (var quote in allList.Where(x => !cache.ContainsKey(x.Id)))
            {
                cache.Add(quote.Id, quote);
            }

            return cache.Values.ToList();
        }

        public void ClearCache()
        {
            cache.Clear();
        }

        public Quote Find(int id)
        {
            if (cache.TryGetValue(id, out var quoteFromCache))
            {
                return quoteFromCache;
            }

            var quote = Connection.QueryFirstOrDefault<Quote>("SELECT * FROM Quotes WHERE Id = @QuoteId LIMIT 1", new { QuoteId = id }, Transaction);
            cache.Add(quote.Id, quote);
            return quote;
        }

        public async Task<Quote> FindAsync(int id)
        {
            if (cache.TryGetValue(id, out var quoteFromCache))
            {
                return quoteFromCache;
            }

            var quote = await Connection.QueryFirstOrDefaultAsync<Quote>("SELECT * FROM Quotes WHERE Id = @QuoteId LIMIT 1", new { QuoteId = id }, Transaction);
            cache.Add(quote.Id, quote);
            return quote;
        }

        public IEnumerable<Quote> GetCachedObjects()
        {
            return cache.Values.ToList();
        }

        public IEnumerable<Quote> GetQuotesFromBook(int bookId)
        {
            var dbResult = Connection.Query<Quote>("SELECT * FROM Quotes WHERE BookId = @BookId", new { BookId = bookId }, Transaction);
            return cache.FilterAndUpdateCache(dbResult);
        }

        public async Task<IEnumerable<Quote>> GetQuotesFromBookAsync(int bookId)
        {
            var dbResult = await Connection.QueryAsync<Quote>("SELECT * FROM Quotes WHERE BookId = @BookId", new { BookId = bookId }, Transaction);
            return cache.FilterAndUpdateCache(dbResult);
        }

        public void Remove(int id)
        {
            Connection.Execute("DELETE FROM Quotes WHERE Id = @RemoveId", new { RemoveId = id }, Transaction);
            cache.Remove(id);
        }

        public void Remove(Quote entity)
        {
            Remove(entity.Id);
            entity.Id = 0;
        }

        public async Task RemoveAsync(int id)
        {
            await Connection.ExecuteAsync("DELETE FROM Quotes WHERE Id = @RemoveId", new { RemoveId = id }, Transaction);
            cache.Remove(id);
        }

        public async Task RemoveAsync(Quote entity)
        {
            await RemoveAsync(entity.Id);
            entity.Id = 0;
        }

        public void Update(Quote entity)
        {
            Connection.Execute("UPDATE Quotes SET Text = @Text, Note = @Note, BookId = @BookId WHERE Id = @Id", entity, Transaction);
        }

        public async Task UpdateAsync(Quote entity)
        {
            await Connection.ExecuteAsync("UPDATE Quotes SET Text = @Text, Note = @Note, BookId = @BookId WHERE Id = @Id", entity, Transaction);
        }
    }
}
