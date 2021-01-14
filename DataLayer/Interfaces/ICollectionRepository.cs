using System.Collections.Generic;
using System.Threading.Tasks;
using Domain;

namespace DataLayer.Interfaces
{
    public interface ICollectionRepository : ICache<UserCollection>
    {
        void Add(UserCollection entity);

        UserCollection GetByTag(string tag);

        void AddCollectionForBook(UserCollection collection, int bookId);

        void RemoveCollectionForBook(UserCollection collection, int bookId);

        IEnumerable<UserCollection> All();

        IEnumerable<UserCollection> GetUserCollectionsOfBook(int bookId);

        int CountUserCollectionsOfBook(int bookId);

        int CountBooksInUserCollection(int collectionId);

        UserCollection Find(int id);

        void Remove(int id);

        void Remove(UserCollection entity);

        void Update(UserCollection entity);

        Task AddAsync(UserCollection entity);

        Task<UserCollection> GetByTagAsync(string tag);

        Task AddCollectionForBookAsync(UserCollection collection, int bookId);

        Task RemoveCollectionForBookAsync(UserCollection collection, int bookId);

        Task<IEnumerable<UserCollection>> AllAsync();

        Task<IEnumerable<UserCollection>> GetUserCollectionsOfBookAsync(int bookId);

        Task<int> CountUserCollectionsOfBookAsync(int bookId);

        Task<int> CountBooksInUserCollectionAsync(int collectionId);

        Task<UserCollection> FindAsync(int id);

        Task RemoveAsync(int id);

        Task RemoveAsync(UserCollection entity);

        Task UpdateAsync(UserCollection entity);
    }
}
