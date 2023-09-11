using System.Collections.Generic;
using System.Threading.Tasks;
using Valyreon.Elib.DataLayer.Filters;
using Valyreon.Elib.Domain;

namespace Valyreon.Elib.DataLayer.Interfaces
{
    public interface IBookRepository : IRepository<Book>
    {
        Task<int> CountAsync(BookFilter filter);

        Task<IEnumerable<Book>> FindByAuthorIdAsync(int authorId);

        Task<IEnumerable<Book>> FindByCollectionIdAsync(int collectionId);

        Task<IEnumerable<Book>> FindBySeriesIdAsync(int seriesId);

        Task<IEnumerable<Book>> GetByFilterAsync(BookFilter filter, int? offset = null, int? pageSize = null);

        Task<Book> GetByPathAsync(string path);

        Task<Book> GetBySignatureAsync(string signature);

        Task<IEnumerable<int>> GetIdsByFilterAsync(BookFilter filter);

        Task<bool> PathExistsAsync(string path);

        Task<bool> SignatureExistsAsync(string signature);
    }
}
