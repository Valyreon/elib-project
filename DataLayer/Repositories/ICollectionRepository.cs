using Domain;
using System.Collections.Generic;

namespace DataLayer.Repositories
{
    public interface ICollectionRepository
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

        void CleanCollections();

        void ClearCache();
    }
}