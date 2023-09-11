using System.Collections.Generic;
using System.Threading.Tasks;
using Valyreon.Elib.Domain;

namespace Valyreon.Elib.DataLayer.Interfaces
{
    public interface ICollectionRepository : IRepository<UserCollection>
    {
        Task AddCollectionForBookAsync(UserCollection collection, int bookId);

        Task<int> CountBooksInUserCollectionAsync(int collectionId);

        Task<int> CountUserCollectionsOfBookAsync(int bookId);

        Task<UserCollection> GetByTagAsync(string tag);

        Task<IEnumerable<UserCollection>> GetUserCollectionsOfBookAsync(int bookId);

        Task RemoveCollectionForBookAsync(UserCollection collection, int bookId);
    }
}
