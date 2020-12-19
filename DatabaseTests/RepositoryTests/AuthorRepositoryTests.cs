using System.Collections.Generic;
using System.Linq;
using DataLayer;
using Domain;
using ElibWpf.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DatabaseTests.RepositoryTests
{
    /// <summary>
    /// The database should not be empty before running this.
    /// </summary>
    [TestClass]
    public class AuthorRepositoryTests
    {
        private readonly List<Author> addedAuthors = new List<Author>();

        [TestInitialize]
        public void Initialize()
        {
            var factory = new UnitOfWorkFactory(ApplicationSettings.GetInstance().DatabasePath);
            using var unitOfWork = factory.Create();

            var one = new Author { Name = "One Author" };
            unitOfWork.AuthorRepository.Add(one);

            var two = new Author { Name = "Two Author" };
            unitOfWork.AuthorRepository.Add(two);

            var three = new Author { Name = "Three Author" };
            unitOfWork.AuthorRepository.Add(three);

            unitOfWork.Commit();

            addedAuthors.AddRange(new Author[] { one, two, three });
        }

        [TestCleanup]
        public void Clean()
        {
            foreach (var author in addedAuthors)
            {
                var factory = new UnitOfWorkFactory(ApplicationSettings.GetInstance().DatabasePath);
                using var unitOfWork = factory.Create();
                unitOfWork.AuthorRepository.Remove(author.Id);
                unitOfWork.Commit();
            }
        }

        [TestMethod]
        public void TestGetAll()
        {
            var factory = new UnitOfWorkFactory(ApplicationSettings.GetInstance().DatabasePath);
            using var unitOfWork = factory.Create();
            var authors = unitOfWork.AuthorRepository.All();

            Assert.IsTrue(authors.Count() >= 3);
            foreach (var x in addedAuthors)
            {
                Assert.IsTrue(authors.Where(a => a.Name == x.Name).Count() == 1);
            }
        }

        [TestMethod]
        public void TestRemoveAndFind()
        {
            var factory = new UnitOfWorkFactory(ApplicationSettings.GetInstance().DatabasePath);
            using (var unitOfWork = factory.Create())
            {
                var found = unitOfWork.AuthorRepository.Find(addedAuthors[0].Id);
                Assert.IsTrue(found.Id == addedAuthors[0].Id && found.Name == addedAuthors[0].Name);
            }

            using (var unitOfWork = factory.Create())
            {
                unitOfWork.AuthorRepository.Remove(addedAuthors[0]);
                unitOfWork.Commit();
            }

            using (var unitOfWork = factory.Create())
            {
                var found = unitOfWork.AuthorRepository.Find(addedAuthors[0].Id);
                Assert.IsTrue(found == null);
            }
        }

        [TestMethod]
        public void TestUpdate()
        {
            var factory = new UnitOfWorkFactory(ApplicationSettings.GetInstance().DatabasePath);
            using (var unitOfWork = factory.Create())
            {
                addedAuthors[0].Name = "Updated";
                unitOfWork.AuthorRepository.Update(addedAuthors[0]);
                unitOfWork.Commit();
            }

            using (var unitOfWork = factory.Create())
            {
                var found = unitOfWork.AuthorRepository.Find(addedAuthors[0].Id);
                Assert.IsTrue(found.Id == addedAuthors[0].Id && found.Name == "Updated");
            }
        }

        [TestMethod]
        public void TestAddAuthorForBook()
        {
            var factory = new UnitOfWorkFactory(ApplicationSettings.GetInstance().DatabasePath);
            var toAdd = new Book
            {
                Title = "Test Book Title",
                FileId = 868
            };

            using (var unitOfWork = factory.Create())
            {
                unitOfWork.BookRepository.Add(toAdd);
                unitOfWork.Commit();
            }

            using (var unitOfWork = factory.Create())
            {
                unitOfWork.AuthorRepository.AddAuthorForBook(addedAuthors[0], toAdd.Id);
                unitOfWork.Commit();
            }

            using (var unitOfWork = factory.Create())
            {
                var authors = unitOfWork.AuthorRepository.GetAuthorsOfBook(toAdd.Id);
                Assert.IsTrue(authors.Count() == 1);
                Assert.IsTrue(authors.First().Id == addedAuthors[0].Id);
            }

            using (var unitOfWork = factory.Create())
            {
                unitOfWork.AuthorRepository.RemoveAuthorForBook(addedAuthors[0], toAdd.Id);
                var authors = unitOfWork.AuthorRepository.GetAuthorsOfBook(toAdd.Id);
                Assert.IsTrue(authors.Count() == 0);
                unitOfWork.AuthorRepository.Remove(addedAuthors[0]);
                unitOfWork.BookRepository.Remove(toAdd);
                unitOfWork.Commit();
            }
        }
    }
}
