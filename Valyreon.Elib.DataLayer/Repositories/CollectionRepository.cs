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
    public class CollectionRepository : RepositoryBase<UserCollection>, ICollectionRepository
    {
        public CollectionRepository(IDbTransaction transaction) : base(transaction)
        {
        }

        public async Task<UserCollection> GetByTagAsync(string tag)
        {
            var fromCache = Cache.Values.FirstOrDefault(x => x.Tag == tag);

            if (fromCache != null)
            {
                return fromCache;
            }

            var fromDb = await Connection.QuerySingleOrDefaultAsync<UserCollection>("SELECT * FROM UserCollections WHERE Tag = @Tag LIMIT 1",
                new { Tag = tag },
                Transaction);

            if (fromDb != null)
            {
                Cache.Add(fromDb.Id, fromDb);
            }

            return fromDb;
        }

        public async Task AddCollectionForBookAsync(UserCollection collection, int bookId)
        {
            if (collection.Id == 0)
            {
                await CreateAsync(collection);
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

        public async Task<IEnumerable<UserCollection>> GetUserCollectionsOfBookAsync(int bookId)
        {
            var dbResult = await Connection.QueryAsync<UserCollection>("SELECT Id, Tag FROM BookId_Collection_View WHERE BookId = @BookId",
                new { BookId = bookId },
                Transaction);

            return Cache.FilterAndUpdateCache(dbResult);
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
    }
}
