using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Valyreon.Elib.DataLayer;
using Valyreon.Elib.Domain;
using Valyreon.Elib.Wpf.Models;

namespace Valyreon.Elib.Tests.RepositoryTests
{
    [TestClass]
    public class CollectionRepositoryTests
    {/*
        private List<UserCollection> addedCollections;

        [TestInitialize]
        public void Initialize()
        {
            var factory = new UnitOfWorkFactory(ApplicationSettings.GetInstance().DatabasePath);
            using var unitOfWork = factory.Create();

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
            var factory = new UnitOfWorkFactory(ApplicationSettings.GetInstance().DatabasePath);
            foreach (var collection in addedCollections)
            {
                using var unitOfWork = factory.Create();
                unitOfWork.CollectionRepository.Remove(collection.Id);
                unitOfWork.Commit();
            }
        }

        [TestMethod]
        public void TestGetAll()
        {
            var factory = new UnitOfWorkFactory(ApplicationSettings.GetInstance().DatabasePath);
            using var unitOfWork = factory.Create();
            var collections = unitOfWork.CollectionRepository.All();

            Assert.IsTrue(collections.Count() >= 3);
            foreach (var x in addedCollections)
            {
                Assert.IsTrue(collections.Count(a => a.Tag == x.Tag) == 1);
            }
        }

        [TestMethod]
        public void TestRemoveAndFind()
        {
            var factory = new UnitOfWorkFactory(ApplicationSettings.GetInstance().DatabasePath);
            using (var unitOfWork = factory.Create())
            {
                var found = unitOfWork.CollectionRepository.Find(addedCollections[0].Id);
                Assert.IsTrue(found.Id == addedCollections[0].Id && found.Tag == addedCollections[0].Tag);
            }

            using (var unitOfWork = factory.Create())
            {
                unitOfWork.CollectionRepository.Remove(addedCollections[0]);
                unitOfWork.Commit();
            }

            using (var unitOfWork = factory.Create())
            {
                var found = unitOfWork.CollectionRepository.Find(addedCollections[0].Id);
                Assert.IsTrue(found == null);
            }
        }

        [TestMethod]
        public void TestUpdate()
        {
            var factory = new UnitOfWorkFactory(ApplicationSettings.GetInstance().DatabasePath);
            using (var unitOfWork = factory.Create())
            {
                addedCollections[0].Tag = "Updated";
                unitOfWork.CollectionRepository.Update(addedCollections[0]);
                unitOfWork.Commit();
            }

            using (var unitOfWork = factory.Create())
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

            var factory = new UnitOfWorkFactory(ApplicationSettings.GetInstance().DatabasePath);
            using (var unitOfWork = factory.Create())
            {
                unitOfWork.BookRepository.Add(toAdd);
                unitOfWork.Commit();
            }

            using (var unitOfWork = factory.Create())
            {
                unitOfWork.CollectionRepository.AddCollectionForBook(addedCollections[0], toAdd.Id);
                unitOfWork.Commit();
            }

            using (var unitOfWork = factory.Create())
            {
                var authors = unitOfWork.CollectionRepository.GetUserCollectionsOfBook(toAdd.Id);
                Assert.IsTrue(authors.Count() == 1);
                Assert.IsTrue(authors.First().Id == addedCollections[0].Id);
            }

            using (var unitOfWork = factory.Create())
            {
                unitOfWork.CollectionRepository.RemoveCollectionForBook(addedCollections[0], toAdd.Id);
                var cols = unitOfWork.CollectionRepository.GetUserCollectionsOfBook(toAdd.Id);
                Assert.IsTrue(!cols.Any());
                unitOfWork.CollectionRepository.Remove(addedCollections[0]);
                unitOfWork.BookRepository.Remove(toAdd);
                unitOfWork.Commit();
            }
        }*/
    }
}
