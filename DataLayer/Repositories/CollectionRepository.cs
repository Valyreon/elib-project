using Dapper;
using Domain;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace DataLayer.Repositories
{
    public class CollectionRepository : RepositoryBase, ICollectionRepository
    {
        private static readonly List<UserCollection> cache = new List<UserCollection>();

        public CollectionRepository(IDbTransaction transaction) : base(transaction)
        {
        }

        public void Add(UserCollection entity)
        {
            entity.Id = Connection.ExecuteScalar<int>(
                "INSERT INTO UserCollections(Tag) VALUES (@Tag); SELECT last_insert_rowid() ",
                entity,
                Transaction
            );

            cache.Add(entity);
        }

        public void AddCollectionForBook(UserCollection collection, int bookId)
        {
            if (collection.Id == 0)
            {
                Add(collection);
            }

            Connection.Execute("INSERT INTO UserCollectionBooks(UserCollectionId, BookId) VALUES (@CollectionId, @BookId)", new { CollectionId = collection.Id, BookId = bookId }, Transaction);
        }

        public IEnumerable<UserCollection> All()
        {
            var allList = Connection.Query<UserCollection>("SELECT * FROM UserCollections", Transaction).AsList();

            foreach (var uc in allList)
            {
                var itemInCache = cache.Find(x => x.Id == uc.Id);
                if (itemInCache == null)
                {
                    cache.Add(uc);
                }
                else
                {
                    itemInCache.Tag = uc.Tag;
                }
            }

            return cache.ToList();
        }

        public void CleanCollections()
        {
            var allCollections = All();
            foreach (var collection in allCollections)
            {
                var count = Connection.QueryFirst<int>(@"SELECT COUNT(*) FROM (
                                                       SELECT UserCollectionBooks.UserCollectionId,
                                                              Books.Id
                                                         FROM Books
                                                              INNER JOIN
                                                              UserCollectionBooks ON Books.Id = UserCollectionBooks.BookId
                                                    ) WHERE Id = @Id"
                , collection, Transaction);

                if (count == 0)
                {
                    Remove(collection.Id);
                }
            }
        }

        public int CountBooksInUserCollection(int collectionId)
        {
            return Connection.QueryFirst<int>("SELECT COUNT(*) FROM UserCollectionBooks WHERE UserCollectionId = @CollectionId", new { CollectionId = collectionId }, Transaction);
        }

        public int CountUserCollectionsOfBook(int bookId)
        {
            return Connection.QueryFirst<int>("SELECT COUNT(*) FROM UserCollectionBooks WHERE BookId = @BookId", new { BookId = bookId }, Transaction);
        }

        public UserCollection Find(int id)
        {
            var cacheResult = cache.Find(x => x.Id == id);

            if (cacheResult != null)
            {
                return cacheResult;
            }

            var result = Connection.QueryFirst<UserCollection>("SELECT * FROM UserCollections WHERE Id = @CollectionId LIMIT 1",
                new { CollectionId = id },
                Transaction);

            if (result != null)
            {
                cache.Add(result);
            }

            return result;
        }

        public UserCollection GetByTag(string tag)
        {
            var fromCache = cache.Find(x => x.Tag == tag);

            if (fromCache != null)
            {
                return fromCache;
            }

            var fromDb = Connection.Query<UserCollection>("SELECT * FROM UserCollections WHERE Tag = @Tag LIMIT 1", new { Tag = tag }, Transaction).FirstOrDefault();

            if (fromDb != null)
            {
                cache.Add(fromDb);
            }

            return fromDb;
        }

        public IEnumerable<UserCollection> GetUserCollectionsOfBook(int bookId)
        {
            var result = new List<UserCollection>();
            var dbResult = Connection.Query<UserCollection>("SELECT Id, Tag FROM BookId_Collection_View WHERE BookId = @BookId", new { BookId = bookId }, Transaction).AsEnumerable();

            foreach (var uc in dbResult)
            {
                var inCache = cache.Find(x => x.Id == uc.Id);
                if (inCache == null)
                {
                    cache.Add(uc);
                    result.Add(uc);
                }
                else
                {
                    inCache.Tag = uc.Tag;
                    result.Add(inCache);
                }
            }

            return result;
        }

        public void Remove(int id)
        {
            Connection.Execute("DELETE FROM UserCollections WHERE Id = @RemoveId", new { RemoveId = id }, Transaction);
            var inCache = cache.Find(x => x.Id == id);
            if (inCache != null)
            {
                cache.Remove(inCache);
            }
        }

        public void Remove(UserCollection entity)
        {
            Remove(entity.Id);
            entity.Id = 0;
        }

        public void RemoveCollectionForBook(UserCollection collection, int bookId)
        {
            Connection.Execute("DELETE FROM UserCollectionBooks WHERE UserCollectionId = @CollectionId AND BookId = @BookId", new { CollectionId = collection.Id, BookId = bookId }, Transaction);
        }

        public void Update(UserCollection entity)
        {
            Connection.Execute("UPDATE UserCollections SET Tag = @Tag WHERE Id = @Id", entity, Transaction);
        }

        public void ClearCache()
        {
            cache.Clear();
        }
    }
}
