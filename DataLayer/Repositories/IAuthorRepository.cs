using Domain;
using System.Collections.Generic;

namespace DataLayer.Repositories
{
    public interface IAuthorRepository
    {
        void Add(Author entity);
        void AddAuthorForBook(Author author, int bookId);
        void RemoveAuthorForBook(Author author, int bookId);
        IEnumerable<Author> All();
        Author Find(int id);
        IEnumerable<Author> GetAuthorsOfBook(int bookId);
        Author GetAuthorWithName(string name);
        int CountBooksByAuthor(int authorId);
        void Remove(int id);
        void Remove(Author entity);
        void Update(Author entity);
        void CleanAuthors();
    }
}
