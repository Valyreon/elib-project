using Dapper;
using Domain;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace DataLayer.Repositories
{
    public class CollectionRepository : RepositoryBase, ICollectionRepository
    {
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
        }

        public void AddCollectionForBook(UserCollection collection, int bookId)
        {
            if (collection.Id == 0)
            {
                this.Add(collection);
            }

            Connection.Execute("INSERT INTO UserCollectionBooks(UserCollectionId, BookId) VALUES (@CollectionId, @BookId)", new { CollectionId = collection.Id, BookId = bookId }, Transaction);
        }

        public IEnumerable<UserCollection> All()
        {
            return Connection.Query<UserCollection>("SELECT * FROM UserCollections", Transaction).AsList();
        }

        public void CleanCollections()
        {
            var allCollections = this.All();
            foreach (var collection in allCollections)
            {
                int count = Connection.QueryFirst<int>(@"SELECT COUNT(*) FROM (
                                                       SELECT UserCollectionBooks.UserCollectionId,
                                                              Books.Id
                                                         FROM Books
                                                              INNER JOIN
                                                              UserCollectionBooks ON Books.Id = UserCollectionBooks.BookId
                                                    ) WHERE Id = @Id"
                , collection, Transaction);

                if (count == 0)
                {
                    this.Remove(collection.Id);
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
            return Connection.Query<UserCollection>("SELECT * FROM UserCollections WHERE Id = @CollectionId LIMIT 1",
                new { CollectionId = id },
                Transaction).FirstOrDefault();
        }

        public UserCollection GetByTag(string tag)
        {
            return Connection.Query<UserCollection>("SELECT * FROM UserCollections WHERE Tag = @Tag", new { Tag = tag }, Transaction).FirstOrDefault();
        }

        public IEnumerable<UserCollection> GetUserCollectionsOfBook(int bookId)
        {
            return Connection.Query<UserCollection>("SELECT Id, Tag FROM BookId_Collection_View WHERE BookId = @BookId", new { BookId = bookId }, Transaction).AsEnumerable();
        }

        public void Remove(int id)
        {
            Connection.Execute("DELETE FROM UserCollections WHERE Id = @RemoveId", new { RemoveId = id }, Transaction);
        }

        public void Remove(UserCollection entity)
        {
            this.Remove(entity.Id);
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
    }
}
