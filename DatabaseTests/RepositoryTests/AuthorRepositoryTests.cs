using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using DataLayer;
using Domain;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Models;

namespace DatabaseTests.RepositoryTests
{
    /// <summary>
    /// The database should not be empty before running this.
    /// </summary>
    [TestClass]
    public class AuthorRepositoryTests
    {
        readonly List<Author> addedAuthors = new List<Author>();

        [TestInitialize]
        public void Initialize()
        {
            using var unitOfWork = new UnitOfWork(ApplicationSettings.GetInstance().DatabasePath);

            Author one = new Author { Name = "One Author" };
            unitOfWork.AuthorRepository.Add(one);

            Author two = new Author { Name = "Two Author" };
            unitOfWork.AuthorRepository.Add(two);

            Author three = new Author { Name = "Three Author" };
            unitOfWork.AuthorRepository.Add(three);

            unitOfWork.Commit();

            addedAuthors.AddRange(new Author[] { one, two, three });
        }

        [TestCleanup]
        public void Clean()
        {
            foreach(var author in addedAuthors)
            {
                using var unitOfWork = new UnitOfWork(ApplicationSettings.GetInstance().DatabasePath);
                unitOfWork.AuthorRepository.Remove(author.Id);
                unitOfWork.Commit();
            }
        }

        [TestMethod]
        public void TestGetAll()
        {
            using var unitOfWork = new UnitOfWork(ApplicationSettings.GetInstance().DatabasePath);
            var authors = unitOfWork.AuthorRepository.All();

            Assert.IsTrue(authors.Count() >= 3);
            foreach (Author x in addedAuthors)
                Assert.IsTrue(authors.Where(a => a.Name == x.Name).Count() == 1);
        }

        [TestMethod]
        public void TestRemoveAndFind()
        {
            using (var unitOfWork = new UnitOfWork(ApplicationSettings.GetInstance().DatabasePath))
            {
                var found = unitOfWork.AuthorRepository.Find(addedAuthors[0].Id);
                Assert.IsTrue(found.Id == addedAuthors[0].Id && found.Name == addedAuthors[0].Name);
            }

            using (var unitOfWork = new UnitOfWork(ApplicationSettings.GetInstance().DatabasePath))
            {
                unitOfWork.AuthorRepository.Remove(addedAuthors[0]);
                unitOfWork.Commit();
            }

            using (var unitOfWork = new UnitOfWork(ApplicationSettings.GetInstance().DatabasePath))
            {
                var found = unitOfWork.AuthorRepository.Find(addedAuthors[0].Id);
                Assert.IsTrue(found == null);
            }
        }

        [TestMethod]
        public void TestUpdate()
        {
            using (var unitOfWork = new UnitOfWork(ApplicationSettings.GetInstance().DatabasePath))
            {
                addedAuthors[0].Name = "Updated";
                unitOfWork.AuthorRepository.Update(addedAuthors[0]);
                unitOfWork.Commit();
            }

            using (var unitOfWork = new UnitOfWork(ApplicationSettings.GetInstance().DatabasePath))
            {
                var found = unitOfWork.AuthorRepository.Find(addedAuthors[0].Id);
                Assert.IsTrue(found.Id == addedAuthors[0].Id && found.Name == "Updated");
            }
        }

        [TestMethod]
        public void TestAddAuthorForBook()
        {
            Book toAdd = new Book
            {
                Title = "Test Book Title",
                FileId = 868
            };

            using (var unitOfWork = new UnitOfWork(ApplicationSettings.GetInstance().DatabasePath))
            {
                unitOfWork.BookRepository.Add(toAdd);
                unitOfWork.Commit();
            }

            using (var unitOfWork = new UnitOfWork(ApplicationSettings.GetInstance().DatabasePath))
            {
                unitOfWork.AuthorRepository.AddAuthorForBook(addedAuthors[0], toAdd.Id);
                unitOfWork.Commit();
            }

            using (var unitOfWork = new UnitOfWork(ApplicationSettings.GetInstance().DatabasePath))
            {
                var authors = unitOfWork.AuthorRepository.GetAuthorsOfBook(toAdd.Id);
                Assert.IsTrue(authors.Count() == 1);
                Assert.IsTrue(authors.First().Id == addedAuthors[0].Id);
            }

            using (var unitOfWork = new UnitOfWork(ApplicationSettings.GetInstance().DatabasePath))
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
