using System.Collections.Generic;
using System.Data;
using System.Linq;
using Dapper;
using DataLayer.Extensions;
using DataLayer.Interfaces;
using Domain;

namespace DataLayer.Repositories
{
    public class AuthorRepository : RepositoryBase, IAuthorRepository
    {
        private static readonly IDictionary<int, Author> cache = new Dictionary<int, Author>();

        public AuthorRepository(IDbTransaction transaction) : base(transaction)
        {
        }

        public void Add(Author entity)
        {
            entity.Id = Connection.ExecuteScalar<int>(
                "INSERT INTO Authors(Name) VALUES (@Name); SELECT last_insert_rowid() ",
                entity,
                Transaction
            );

            cache.Add(entity.Id, entity);
        }

        public void AddAuthorForBook(Author author, int bookId)
        {
            if (author.Id == 0)
            {
                Add(author);
            }

            Connection.Execute("INSERT INTO AuthorBooks(AuthorId, BookId) VALUES (@AuthorId, @BookId)", new { AuthorId = author.Id, BookId = bookId }, Transaction);
        }

        public IEnumerable<Author> All()
        {
            var allList = Connection.Query<Author>("SELECT * FROM Authors", Transaction);

            foreach (var author in allList.Where(x => !cache.ContainsKey(x.Id)))
            {
                cache.Add(author.Id, author);
            }

            return cache.Values.ToList();
        }

        public void ClearCache()
        {
            cache.Clear();
        }

        public int CountBooksByAuthor(int authorId)
        {
            return Connection.QueryFirst<int>(@"SELECT COUNT(*) FROM AuthorBooks WHERE AuthorId = @AuthorId", new { AuthorId = authorId }, Transaction);
        }

        public Author Find(int id)
        {
            if (cache.TryGetValue(id, out var authorFromCache))
            {
                return authorFromCache;
            }

            var result = Connection.QueryFirst<Author>("SELECT * FROM Authors WHERE Id = @AuthorId LIMIT 1",
                new { AuthorId = id },
                Transaction);

            if (result != null)
            {
                cache.Add(result.Id, result);
            }

            return result;
        }

        public IEnumerable<Author> GetAuthorsOfBook(int bookId)
        {
            var dbResult = Connection.Query<Author>("SELECT Id, Name FROM BookId_Author_View WHERE BookId = @BookId", new { BookId = bookId }, Transaction);
            return cache.FilterAndUpdateCache(dbResult);
        }

        public Author GetAuthorWithName(string name)
        {
            var result = cache.Values.FirstOrDefault(x => x.Name == name);

            if (result != null)
            {
                return result;
            }

            return Connection.QueryFirst<Author>("SELECT * FROM Authors WHERE Name = @AuthorName LIMIT 1", new { AuthorName = name }, Transaction);
        }

        public IEnumerable<Author> GetCachedObjects()
        {
            return cache.Values.ToList();
        }

        public void Remove(int id)
        {
            Connection.Execute("DELETE FROM Authors WHERE Id = @RemoveId", new { RemoveId = id }, Transaction);
            cache.Remove(id);
        }

        public void Remove(Author entity)
        {
            Remove(entity.Id);
            entity.Id = 0;
        }

        public void RemoveAuthorForBook(Author author, int bookId)
        {
            Connection.Execute("DELETE FROM AuthorBooks WHERE AuthorId = @AuthorId AND BookId = @BookId", new { AuthorId = author.Id, BookId = bookId }, Transaction);
            cache.Remove(author.Id);
        }

        public void Update(Author entity)
        {
            Connection.Execute("UPDATE Authors SET Name = @Name WHERE Id = @Id", entity, Transaction);
        }
    }
}
