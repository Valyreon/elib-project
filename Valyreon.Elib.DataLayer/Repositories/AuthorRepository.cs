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

        public async Task AddAsync(Author entity)
        {
            entity.Id = await Connection.ExecuteScalarAsync<int>(
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

        public async Task AddAuthorForBookAsync(Author author, int bookId)
        {
            if (author.Id == 0)
            {
                Add(author);
            }

            await Connection.ExecuteAsync("INSERT INTO AuthorBooks(AuthorId, BookId) VALUES (@AuthorId, @BookId)", new { AuthorId = author.Id, BookId = bookId }, Transaction);
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

        public async Task<IEnumerable<Author>> AllAsync()
        {
            var allList = await Connection.QueryAsync<Author>("SELECT * FROM Authors", Transaction);

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
            return Connection.QueryFirst<int>("SELECT COUNT(*) FROM AuthorBooks WHERE AuthorId = @AuthorId", new { AuthorId = authorId }, Transaction);
        }

        public async Task<int> CountBooksByAuthorAsync(int authorId)
        {
            return await Connection.QueryFirstAsync<int>("SELECT COUNT(*) FROM AuthorBooks WHERE AuthorId = @AuthorId", new { AuthorId = authorId }, Transaction);
        }

        public Author Find(int id)
        {
            if (cache.TryGetValue(id, out var authorFromCache))
            {
                return authorFromCache;
            }

            var result = Connection.QueryFirstOrDefault<Author>("SELECT * FROM Authors WHERE Id = @AuthorId LIMIT 1",
                new { AuthorId = id },
                Transaction);

            if (result != null)
            {
                cache.Add(result.Id, result);
            }

            return result;
        }

        public async Task<Author> FindAsync(int id)
        {
            if (cache.TryGetValue(id, out var authorFromCache))
            {
                return authorFromCache;
            }

            var result = await Connection.QueryFirstOrDefaultAsync<Author>("SELECT * FROM Authors WHERE Id = @AuthorId LIMIT 1",
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

        public async Task<IEnumerable<Author>> GetAuthorsOfBookAsync(int bookId)
        {
            var dbResult = await Connection.QueryAsync<Author>("SELECT Id, Name FROM BookId_Author_View WHERE BookId = @BookId", new { BookId = bookId }, Transaction);
            return cache.FilterAndUpdateCache(dbResult);
        }

        public Author GetAuthorWithName(string name)
        {
            var result = cache.Values.FirstOrDefault(x => x.Name == name);

            return result ?? Connection.QueryFirstOrDefault<Author>("SELECT * FROM Authors WHERE Name = @AuthorName LIMIT 1", new { AuthorName = name }, Transaction);
        }

        public async Task<Author> GetAuthorWithNameAsync(string name)
        {
            var result = cache.Values.FirstOrDefault(x => x.Name == name);

            return result ?? await Connection.QueryFirstOrDefaultAsync<Author>("SELECT * FROM Authors WHERE Name = @AuthorName LIMIT 1", new { AuthorName = name }, Transaction);
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

        public async Task RemoveAsync(int id)
        {
            await Connection.ExecuteAsync("DELETE FROM Authors WHERE Id = @RemoveId", new { RemoveId = id }, Transaction);
            cache.Remove(id);
        }

        public async Task RemoveAsync(Author entity)
        {
            await RemoveAsync(entity.Id);
            entity.Id = 0;
        }

        public void RemoveAuthorForBook(Author author, int bookId)
        {
            Connection.Execute("DELETE FROM AuthorBooks WHERE AuthorId = @AuthorId AND BookId = @BookId", new { AuthorId = author.Id, BookId = bookId }, Transaction);
            cache.Remove(author.Id);
        }

        public async Task RemoveAuthorForBookAsync(Author author, int bookId)
        {
            await Connection.ExecuteAsync("DELETE FROM AuthorBooks WHERE AuthorId = @AuthorId AND BookId = @BookId", new { AuthorId = author.Id, BookId = bookId }, Transaction);
            cache.Remove(author.Id);
        }

        public void Update(Author entity)
        {
            Connection.Execute("UPDATE Authors SET Name = @Name WHERE Id = @Id", entity, Transaction);
        }

        public async Task UpdateAsync(Author entity)
        {
            await Connection.ExecuteAsync("UPDATE Authors SET Name = @Name WHERE Id = @Id", entity, Transaction);
        }
    }
}
