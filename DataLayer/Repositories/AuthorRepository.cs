using Dapper;
using Domain;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace DataLayer.Repositories
{
    public class AuthorRepository : RepositoryBase, IAuthorRepository
    {
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
        }

        public void AddAuthorForBook(Author author, int bookId)
        {
            if(author.Id == 0)
            {
                this.Add(author);
            }

            Connection.Execute("INSERT INTO AuthorBooks(AuthorId, BookId) VALUES (@AuthorId, @BookId)", new { AuthorId = author.Id, BookId = bookId }, Transaction);
        }

        public IEnumerable<Author> All()
        {
            return Connection.Query<Author>("SELECT * FROM Authors", Transaction).AsList();
        }

        public void CleanAuthors()
        {
            var allAuthors = this.All();
            foreach(var author in allAuthors)
            {
                int count = Connection.QueryFirst<int>(@"SELECT COUNT(*) FROM (
                                                       SELECT AuthorBooks.AuthorId,
                                                              Books.Id
                                                         FROM Books
                                                              INNER JOIN
                                                              AuthorBooks ON Books.Id = AuthorBooks.BookId
                                                    ) WHERE Id = @Id"
                , author, Transaction);

                if(count == 0)
                {
                    this.Remove(author.Id);
                }
            }
        }

        public int CountBooksByAuthor(int authorId)
        {
            return Connection.QueryFirst<int>(@"SELECT COUNT(*) FROM AuthorBooks WHERE AuthorId = @AuthorId", new { AuthorId = authorId }, Transaction);
        }

        public Author Find(int id)
        {
            return Connection.Query<Author>("SELECT * FROM Authors WHERE Id = @AuthorId LIMIT 1",
                new { AuthorId = id },
                Transaction).FirstOrDefault();
        }

        public IEnumerable<Author> GetAuthorsOfBook(int bookId)
        {
            return Connection.Query<Author>("SELECT Id, Name FROM BookId_Author_View WHERE BookId = @BookId", new { BookId = bookId }, Transaction).AsEnumerable();
        }

        public Author GetAuthorWithName(string name)
        {
            return Connection.Query<Author>("SELECT * FROM Authors WHERE Name = @AuthorName LIMIT 1",
                new { AuthorName = name },
                Transaction).FirstOrDefault();
        }

        public void Remove(int id)
        {
            Connection.Execute("DELETE FROM Authors WHERE Id = @RemoveId", new { RemoveId = id }, Transaction);
        }

        public void Remove(Author entity)
        {
            this.Remove(entity.Id);
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
