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
    public class CollectionRepository : RepositoryBase, ICollectionRepository
    {
        private static readonly IDictionary<int, UserCollection> cache = new Dictionary<int, UserCollection>();

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

            cache.Add(entity.Id, entity);
        }

        public void AddCollectionForBook(UserCollection collection, int bookId)
        {
            if (collection.Id == 0)
            {
                Add(collection);
            }

            Connection.Execute("INSERT INTO UserCollectionBooks(UserCollectionId, BookId) VALUES (@CollectionId, @BookId)",
                new { CollectionId = collection.Id, BookId = bookId },
                Transaction);
        }

        public IEnumerable<UserCollection> All()
        {
            var allList = Connection.Query<UserCollection>("SELECT * FROM UserCollections", Transaction);
            return cache.FilterAndUpdateCache(allList);
        }

        public int CountBooksInUserCollection(int collectionId)
        {
            return Connection.QueryFirst<int>("SELECT COUNT(*) FROM UserCollectionBooks WHERE UserCollectionId = @CollectionId",
                new { CollectionId = collectionId },
                Transaction);
        }

        public int CountUserCollectionsOfBook(int bookId)
        {
            return Connection.QueryFirst<int>("SELECT COUNT(*) FROM UserCollectionBooks WHERE BookId = @BookId",
                new { BookId = bookId },
                Transaction);
        }

        public UserCollection Find(int id)
        {
            if (cache.TryGetValue(id, out var collectionFromCache))
            {
                return collectionFromCache;
            }

            var result = Connection.QueryFirst<UserCollection>("SELECT * FROM UserCollections WHERE Id = @CollectionId LIMIT 1",
                new { CollectionId = id },
                Transaction);

            if (result != null)
            {
                cache.Add(result.Id, result);
            }

            return result;
        }

        public UserCollection GetByTag(string tag)
        {
            var fromCache = cache.Values.FirstOrDefault(x => x.Tag == tag);

            if (fromCache != null)
            {
                return fromCache;
            }

            var fromDb = Connection.QuerySingleOrDefault<UserCollection>("SELECT * FROM UserCollections WHERE Tag = @Tag LIMIT 1",
                new { Tag = tag },
                Transaction);

            if (fromDb != null)
            {
                cache.Add(fromDb.Id, fromDb);
            }

            return fromDb;
        }

        public IEnumerable<UserCollection> GetUserCollectionsOfBook(int bookId)
        {
            var dbResult = Connection.Query<UserCollection>("SELECT Id, Tag FROM BookId_Collection_View WHERE BookId = @BookId",
                new { BookId = bookId },
                Transaction);
            return cache.FilterAndUpdateCache(dbResult);
        }

        public void Remove(int id)
        {
            Connection.Execute("DELETE FROM UserCollections WHERE Id = @RemoveId", new { RemoveId = id }, Transaction);
            cache.Remove(id);
        }

        public void Remove(UserCollection entity)
        {
            Remove(entity.Id);
            entity.Id = 0;
        }

        public void RemoveCollectionForBook(UserCollection collection, int bookId)
        {
            Connection.Execute("DELETE FROM UserCollectionBooks WHERE UserCollectionId = @CollectionId AND BookId = @BookId",
                new { CollectionId = collection.Id, BookId = bookId },
                Transaction);
        }

        public void Update(UserCollection entity)
        {
            Connection.Execute("UPDATE UserCollections SET Tag = @Tag WHERE Id = @Id", entity, Transaction);
        }

        public void ClearCache()
        {
            cache.Clear();
        }

        public IEnumerable<UserCollection> GetCachedObjects()
        {
            return cache.Values.ToList();
        }

        public async Task AddAsync(UserCollection entity)
        {
            entity.Id = await Connection.ExecuteScalarAsync<int>(
                "INSERT INTO UserCollections(Tag) VALUES (@Tag); SELECT last_insert_rowid() ",
                entity,
                Transaction
            ).ConfigureAwait(false);

            cache.Add(entity.Id, entity);
        }

        public async Task<UserCollection> GetByTagAsync(string tag)
        {
            var fromCache = cache.Values.FirstOrDefault(x => x.Tag == tag);

            if (fromCache != null)
            {
                return fromCache;
            }

            var fromDb = await Connection.QuerySingleOrDefaultAsync<UserCollection>("SELECT * FROM UserCollections WHERE Tag = @Tag LIMIT 1",
                new { Tag = tag },
                Transaction);

            if (fromDb != null)
            {
                cache.Add(fromDb.Id, fromDb);
            }

            return fromDb;
        }

        public async Task AddCollectionForBookAsync(UserCollection collection, int bookId)
        {
            if (collection.Id == 0)
            {
                Add(collection);
            }

            await Connection.ExecuteAsync("INSERT INTO UserCollectionBooks(UserCollectionId, BookId) VALUES (@CollectionId, @BookId)",
                new { CollectionId = collection.Id, BookId = bookId },
                Transaction);
        }

        public async Task RemoveCollectionForBookAsync(UserCollection collection, int bookId)
        {
            await Connection.ExecuteAsync("DELETE FROM UserCollectionBooks WHERE UserCollectionId = @CollectionId AND BookId = @BookId",
                new { CollectionId = collection.Id, BookId = bookId },
                Transaction);
        }

        public async Task<IEnumerable<UserCollection>> AllAsync()
        {
            var allList = await Connection.QueryAsync<UserCollection>("SELECT * FROM UserCollections", Transaction);
            return cache.FilterAndUpdateCache(allList);
        }

        public async Task<IEnumerable<UserCollection>> GetUserCollectionsOfBookAsync(int bookId)
        {
            var dbResult = await Connection.QueryAsync<UserCollection>("SELECT Id, Tag FROM BookId_Collection_View WHERE BookId = @BookId",
                new { BookId = bookId },
                Transaction);
            return cache.FilterAndUpdateCache(dbResult);
        }

        public async Task<int> CountUserCollectionsOfBookAsync(int bookId)
        {
            return await Connection.QueryFirstAsync<int>("SELECT COUNT(*) FROM UserCollectionBooks WHERE BookId = @BookId",
                new { BookId = bookId },
                Transaction);
        }

        public async Task<int> CountBooksInUserCollectionAsync(int collectionId)
        {
            return await Connection.QueryFirstAsync<int>("SELECT COUNT(*) FROM UserCollectionBooks WHERE UserCollectionId = @CollectionId",
                new { CollectionId = collectionId },
                Transaction);
        }

        public async Task<UserCollection> FindAsync(int id)
        {
            if (cache.TryGetValue(id, out var collectionFromCache))
            {
                return collectionFromCache;
            }

            var result = await Connection.QuerySingleOrDefaultAsync<UserCollection>("SELECT * FROM UserCollections WHERE Id = @CollectionId LIMIT 1",
                new { CollectionId = id },
                Transaction);

            if (result != null)
            {
                cache.Add(result.Id, result);
            }

            return result;
        }

        public async Task RemoveAsync(int id)
        {
            await Connection.ExecuteAsync("DELETE FROM UserCollections WHERE Id = @RemoveId", new { RemoveId = id }, Transaction);
            cache.Remove(id);
        }

        public async Task RemoveAsync(UserCollection entity)
        {
            await RemoveAsync(entity.Id);
            entity.Id = 0;
        }

        public async Task UpdateAsync(UserCollection entity)
        {
            await Connection.ExecuteAsync("UPDATE UserCollections SET Tag = @Tag WHERE Id = @Id", entity, Transaction);
        }
    }
}
