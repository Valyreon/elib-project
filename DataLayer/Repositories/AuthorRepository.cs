using Dapper;
using Domain;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace DataLayer.Repositories
{
    public class AuthorRepository : RepositoryBase, IAuthorRepository
    {
        private static readonly List<Author> cache = new List<Author>();

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

            cache.Add(entity);
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

            foreach (var author in allList)
            {
                var itemInCache = cache.Find(x => x.Id == author.Id);
                if (itemInCache == null)
                {
                    cache.Add(author);
                }
                else
                {
                    itemInCache.Name = author.Name;
                }
            }

            return cache.ToList();
        }

        public void CleanAuthors()
        {
            var allAuthors = All();
            foreach (var author in allAuthors)
            {
                var count = Connection.QueryFirst<int>(@"SELECT COUNT(*) FROM (
                                                       SELECT AuthorBooks.AuthorId,
                                                              Books.Id
                                                         FROM Books
                                                              INNER JOIN
                                                              AuthorBooks ON Books.Id = AuthorBooks.BookId
                                                    ) WHERE Id = @Id"
                , author, Transaction);

                if (count == 0)
                {
                    Remove(author.Id);
                }
            }
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
            var cacheResult = cache.Find(s => s.Id == id);
            if (cacheResult != null)
            {
                return cacheResult;
            }

            var result = Connection.Query<Author>("SELECT * FROM Authors WHERE Id = @AuthorId LIMIT 1",
                new { AuthorId = id },
                Transaction).FirstOrDefault();

            if (result != null)
            {
                cache.Add(result);
            }

            return result;
        }

        public IEnumerable<Author> GetAuthorsOfBook(int bookId)
        {
            var result = new List<Author>();
            var dbResult = Connection.Query<Author>("SELECT Id, Name FROM BookId_Author_View WHERE BookId = @BookId", new { BookId = bookId }, Transaction);

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
                    inCache.Name = uc.Name;
                    result.Add(inCache);
                }
            }

            return result;
        }

        public Author GetAuthorWithName(string name)
        {
            var results = cache.Find(x => x.Name == name);
            return results ?? Connection.Query<Author>("SELECT * FROM Authors WHERE Name = @AuthorName LIMIT 1",
                                                new { AuthorName = name },
                                                Transaction).FirstOrDefault();
        }

        public void Remove(int id)
        {
            Connection.Execute("DELETE FROM Authors WHERE Id = @RemoveId", new { RemoveId = id }, Transaction);
            var inCache = cache.Find(x => x.Id == id);
            if (inCache != null)
            {
                cache.Remove(inCache);
            }
        }

        public void Remove(Author entity)
        {
            Remove(entity.Id);
            entity.Id = 0;
        }

        public void RemoveAuthorForBook(Author author, int bookId)
        {
            Connection.Execute("DELETE FROM AuthorBooks WHERE AuthorId = @AuthorId AND BookId = @BookId", new { AuthorId = author.Id, BookId = bookId }, Transaction);
        }

        public void Update(Author entity)
        {
            Connection.Execute("UPDATE Authors SET Name = @Name WHERE Id = @Id", entity, Transaction);
        }
    }
}
