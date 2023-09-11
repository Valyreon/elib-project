using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Valyreon.Elib.DataLayer;
using Valyreon.Elib.Domain;
using Valyreon.Elib.Wpf.Models;

namespace Valyreon.Elib.Tests.RepositoryTests
{
    /// <summary>
    /// The database should not be empty before running this.
    /// </summary>
    [TestClass]
    public class AuthorRepositoryTests
    {
        private readonly List<Author> addedAuthors = new List<Author>();

        [TestCleanup]
        public async Task Clean()
        {
            foreach (var author in addedAuthors)
            {
                var factory = new UnitOfWorkFactory(ApplicationData.DatabasePath);
                using var unitOfWork = await factory.CreateAsync();
                await unitOfWork.AuthorRepository.DeleteAsync(author.Id);
                unitOfWork.Commit();
            }
        }

        [TestInitialize]
        public async Task Initialize()
        {
            var factory = new UnitOfWorkFactory(ApplicationData.DatabasePath);
            using var unitOfWork = await factory.CreateAsync();

            var one = new Author { Name = "One Author" };
            await unitOfWork.AuthorRepository.CreateAsync(one);

            var two = new Author { Name = "Two Author" };
            await unitOfWork.AuthorRepository.CreateAsync(two);

            var three = new Author { Name = "Three Author" };
            await unitOfWork.AuthorRepository.CreateAsync(three);

            unitOfWork.Commit();

            addedAuthors.AddRange(new Author[] { one, two, three });
        }

        [TestMethod]
        public async Task TestAddAuthorForBook()
        {
            var factory = new UnitOfWorkFactory(ApplicationData.DatabasePath);
            var toAdd = new Book
            {
                Title = "Test Book Title",
            };

            using (var unitOfWork = await factory.CreateAsync())
            {
                await unitOfWork.BookRepository.CreateAsync(toAdd);
                unitOfWork.Commit();
            }

            using (var unitOfWork = await factory.CreateAsync())
            {
                await unitOfWork.AuthorRepository.AddAuthorForBookAsync(addedAuthors[0], toAdd.Id);
                unitOfWork.Commit();
            }

            using (var unitOfWork = await factory.CreateAsync())
            {
                var authors = await unitOfWork.AuthorRepository.GetAuthorsOfBookAsync(toAdd.Id);
                Assert.IsTrue(authors.Count() == 1);
                Assert.IsTrue(authors.First().Id == addedAuthors[0].Id);
            }

            using (var unitOfWork = await factory.CreateAsync())
            {
                await unitOfWork.AuthorRepository.RemoveAuthorForBookAsync(addedAuthors[0], toAdd.Id);
                var authors = await unitOfWork.AuthorRepository.GetAuthorsOfBookAsync(toAdd.Id);
                Assert.IsTrue(!authors.Any());
                await unitOfWork.AuthorRepository.DeleteAsync(addedAuthors[0]);
                await unitOfWork.BookRepository.DeleteAsync(toAdd);
                unitOfWork.Commit();
            }
        }

        [TestMethod]
        public async Task TestGetAll()
        {
            var factory = new UnitOfWorkFactory(ApplicationData.DatabasePath);
            using var unitOfWork = await factory.CreateAsync();
            var authors = await unitOfWork.AuthorRepository.GetAllAsync();

            Assert.IsTrue(authors.Count() >= 3);
            foreach (var x in addedAuthors)
            {
                Assert.IsTrue(authors.Count(a => a.Name == x.Name) == 1);
            }
        }

        [TestMethod]
        public async Task TestRemoveAndFind()
        {
            var factory = new UnitOfWorkFactory(ApplicationData.DatabasePath);
            using (var unitOfWork = await factory.CreateAsync())
            {
                var found = await unitOfWork.AuthorRepository.FindAsync(addedAuthors[0].Id);
                Assert.IsTrue(found.Id == addedAuthors[0].Id && found.Name == addedAuthors[0].Name);
            }

            using (var unitOfWork = await factory.CreateAsync())
            {
                await unitOfWork.AuthorRepository.DeleteAsync(addedAuthors[0]);
                unitOfWork.Commit();
            }

            using (var unitOfWork = await factory.CreateAsync())
            {
                var found = await unitOfWork.AuthorRepository.FindAsync(addedAuthors[0].Id);
                Assert.IsTrue(found == null);
            }
        }

        [TestMethod]
        public async Task TestUpdate()
        {
            var factory = new UnitOfWorkFactory(ApplicationData.DatabasePath);
            using (var unitOfWork = await factory.CreateAsync())
            {
                addedAuthors[0].Name = "Updated";
                await unitOfWork.AuthorRepository.UpdateAsync(addedAuthors[0]);
                unitOfWork.Commit();
            }

            using (var unitOfWork = await factory.CreateAsync())
            {
                var found = await unitOfWork.AuthorRepository.FindAsync(addedAuthors[0].Id);
                Assert.IsTrue(found.Id == addedAuthors[0].Id && found.Name == "Updated");
            }
        }
    }
}
