using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using Valyreon.Elib.DataLayer.Interfaces;
using Valyreon.Elib.Domain;

namespace Valyreon.Elib.DataLayer.Repositories
{
    public class CollectionRepository : RepositoryBase<UserCollection>, ICollectionRepository
    {
        public CollectionRepository(IDbTransaction transaction) : base(transaction)
        {
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

        public Task<int> CountBooksInUserCollectionAsync(int collectionId)
        {
            return Connection.QueryFirstAsync<int>("SELECT COUNT(*) FROM UserCollectionBooks WHERE UserCollectionId = @CollectionId",
                new { CollectionId = collectionId },
                Transaction);
        }

        public Task<int> CountUserCollectionsOfBookAsync(int bookId)
        {
            return Connection.QueryFirstAsync<int>("SELECT COUNT(*) FROM UserCollectionBooks WHERE BookId = @BookId",
                new { BookId = bookId },
                Transaction);
        }

        public Task<UserCollection> GetByTagAsync(string tag)
        {
            return Connection.QuerySingleOrDefaultAsync<UserCollection>("SELECT * FROM UserCollections WHERE Tag = @Tag LIMIT 1",
                 new { Tag = tag },
                 Transaction);
        }

        public Task<IEnumerable<UserCollection>> GetUserCollectionsOfBookAsync(int bookId)
        {
            return Connection.QueryAsync<UserCollection>("SELECT Id, Tag FROM BookId_Collection_View WHERE BookId = @BookId",
                new { BookId = bookId },
                Transaction);
        }

        public Task RemoveCollectionForBookAsync(UserCollection collection, int bookId)
        {
            return Connection.ExecuteAsync("DELETE FROM UserCollectionBooks WHERE UserCollectionId = @CollectionId AND BookId = @BookId",
                new { CollectionId = collection.Id, BookId = bookId },
                Transaction);
        }
    }
}
