using System.Collections.Generic;
using System.Threading.Tasks;
using Valyreon.Elib.Domain;

namespace Valyreon.Elib.DataLayer.Interfaces
{
    public interface IBookRepository : IRepository<Book>
    {
        Task<IEnumerable<Book>> FindBySeriesIdAsync(int seriesId);

        Task<IEnumerable<Book>> FindByCollectionIdAsync(int collectionId);

        Task<IEnumerable<Book>> FindByAuthorIdAsync(int authorId);

        Task<IEnumerable<Book>> GetByFilterAsync(FilterParameters filter, int? offset = null, int? pageSize = null);

        Task<IEnumerable<int>> GetIdsByFilterAsync(FilterParameters filter);

        Task<int> CountAsync(FilterParameters filter);

        Task<bool> SignatureExistsAsync(string signature);

        Task<bool> PathExistsAsync(string path);

        Task<Book> GetByPathAsync(string path);

        Task<Book> GetBySignatureAsync(string signature);
    }
}
