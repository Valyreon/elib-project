using System.Collections.Generic;
using System.Threading.Tasks;
using Valyreon.Elib.Domain;

namespace Valyreon.Elib.DataLayer.Interfaces
{
    public interface IQuoteRepository : ICache<Quote>
    {
        void Add(Quote entity);

        IEnumerable<Quote> All();

        Quote Find(int id);

        IEnumerable<Quote> GetQuotesFromBook(int bookId);

        void Remove(int id);

        void Remove(Quote entity);

        void Update(Quote entity);

        Task AddAsync(Quote entity);

        Task<IEnumerable<Quote>> AllAsync();

        Task<Quote> FindAsync(int id);

        Task<IEnumerable<Quote>> GetQuotesFromBookAsync(int bookId);

        Task RemoveAsync(int id);

        Task RemoveAsync(Quote entity);

        Task UpdateAsync(Quote entity);
    }
}
