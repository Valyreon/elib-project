using DataLayer;
using Domain;
using ElibWpf.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;

namespace DatabaseTests.RepositoryTests
{
    [TestClass]
    public class CollectionRepositoryTests
    {
        private List<UserCollection> addedCollections;

        [TestInitialize]
        public void Initialize()
        {
            using var unitOfWork = new UnitOfWork(ApplicationSettings.GetInstance().DatabasePath);

            addedCollections = new List<UserCollection>
            {
                new UserCollection { Tag = "One Collection" },
                new UserCollection { Tag = "Two Collection" },
                new UserCollection { Tag = "Three Collection" }
            };

            foreach (var col in addedCollections)
            {
                unitOfWork.CollectionRepository.Add(col);
            }

            unitOfWork.Commit();
        }

        [TestCleanup]
        public void Clean()
        {
            foreach (var collection in addedCollections)
            {
                using var unitOfWork = new UnitOfWork(ApplicationSettings.GetInstance().DatabasePath);
                unitOfWork.CollectionRepository.Remove(collection.Id);
                unitOfWork.Commit();
            }
        }

        [TestMethod]
        public void TestGetAll()
        {
            using var unitOfWork = new UnitOfWork(ApplicationSettings.GetInstance().DatabasePath);
            var collections = unitOfWork.CollectionRepository.All();

            Assert.IsTrue(collections.Count() >= 3);
            foreach (var x in addedCollections)
            {
                Assert.IsTrue(collections.Where(a => a.Tag == x.Tag).Count() == 1);
            }
        }

        [TestMethod]
        public void TestRemoveAndFind()
        {
            using (var unitOfWork = new UnitOfWork(ApplicationSettings.GetInstance().DatabasePath))
            {
                var found = unitOfWork.CollectionRepository.Find(addedCollections[0].Id);
                Assert.IsTrue(found.Id == addedCollections[0].Id && found.Tag == addedCollections[0].Tag);
            }

            using (var unitOfWork = new UnitOfWork(ApplicationSettings.GetInstance().DatabasePath))
            {
                unitOfWork.CollectionRepository.Remove(addedCollections[0]);
                unitOfWork.Commit();
            }

            using (var unitOfWork = new UnitOfWork(ApplicationSettings.GetInstance().DatabasePath))
            {
                var found = unitOfWork.CollectionRepository.Find(addedCollections[0].Id);
                Assert.IsTrue(found == null);
            }
        }

        [TestMethod]
        public void TestUpdate()
        {
            using (var unitOfWork = new UnitOfWork(ApplicationSettings.GetInstance().DatabasePath))
            {
                addedCollections[0].Tag = "Updated";
                unitOfWork.CollectionRepository.Update(addedCollections[0]);
                unitOfWork.Commit();
            }

            using (var unitOfWork = new UnitOfWork(ApplicationSettings.GetInstance().DatabasePath))
            {
                var found = unitOfWork.CollectionRepository.Find(addedCollections[0].Id);
                Assert.IsTrue(found.Id == addedCollections[0].Id && found.Tag == "Updated");
            }
        }

        [TestMethod]
        public void TestAddCollectionForBook()
        {
            var toAdd = new Book
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
                unitOfWork.CollectionRepository.AddCollectionForBook(addedCollections[0], toAdd.Id);
                unitOfWork.Commit();
            }

            using (var unitOfWork = new UnitOfWork(ApplicationSettings.GetInstance().DatabasePath))
            {
                var authors = unitOfWork.CollectionRepository.GetUserCollectionsOfBook(toAdd.Id);
                Assert.IsTrue(authors.Count() == 1);
                Assert.IsTrue(authors.First().Id == addedCollections[0].Id);
            }

            using (var unitOfWork = new UnitOfWork(ApplicationSettings.GetInstance().DatabasePath))
            {
                unitOfWork.CollectionRepository.RemoveCollectionForBook(addedCollections[0], toAdd.Id);
                var cols = unitOfWork.CollectionRepository.GetUserCollectionsOfBook(toAdd.Id);
                Assert.IsTrue(cols.Count() == 0);
                unitOfWork.CollectionRepository.Remove(addedCollections[0]);
                unitOfWork.BookRepository.Remove(toAdd);
                unitOfWork.Commit();
            }
        }
    }
}
