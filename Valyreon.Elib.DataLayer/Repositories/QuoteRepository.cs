using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Valyreon.Elib.DataLayer.Extensions;
using Valyreon.Elib.DataLayer.Interfaces;
using Valyreon.Elib.Domain;

namespace Valyreon.Elib.DataLayer.Repositories
{
    public class QuoteRepository : RepositoryBase<Quote>, IQuoteRepository
    {
        public QuoteRepository(IDbTransaction transaction) : base(transaction)
        {
        }
        
        public async Task<IEnumerable<Quote>> GetQuotesFromBookAsync(int bookId)
        {
            var dbResult = await Connection.QueryAsync<Quote>("SELECT * FROM Quotes WHERE BookId = @BookId", new { BookId = bookId }, Transaction);
            return Cache.FilterAndUpdateCache(dbResult);
        }
    }
}
