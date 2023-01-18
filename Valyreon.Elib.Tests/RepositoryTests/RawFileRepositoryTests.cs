using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Valyreon.Elib.DataLayer;
using Valyreon.Elib.Domain;
using Valyreon.Elib.Wpf.Models;

namespace Valyreon.Elib.Tests.RepositoryTests
{
    /*
    [TestClass]
    public class RawFileRepositoryTests
    {
        private List<RawFile> addedFiles;

        [TestInitialize]
        public void Initialize()
        {
            var factory = new UnitOfWorkFactory(ApplicationSettings.GetInstance().DatabasePath);
            using var unitOfWork = factory.Create();

            addedFiles = new List<RawFile>
            {
                new RawFile { RawContent = Encoding.ASCII.GetBytes("One File") },
                new RawFile { RawContent = Encoding.ASCII.GetBytes("Two File") },
                new RawFile { RawContent = Encoding.ASCII.GetBytes("Three File") }
            };

            foreach (var col in addedFiles)
            {
                unitOfWork.RawFileRepository.Add(col);
            }

            unitOfWork.Commit();
        }

        [TestCleanup]
        public void Clean()
        {
            foreach (var file in addedFiles)
            {
                var factory = new UnitOfWorkFactory(ApplicationSettings.GetInstance().DatabasePath);
                using var unitOfWork = factory.Create();
                unitOfWork.RawFileRepository.Remove(file.Id);
                unitOfWork.Commit();
            }
        }

        [TestMethod]
        public void TestFind()
        {
            var factory = new UnitOfWorkFactory(ApplicationSettings.GetInstance().DatabasePath);
            using var uow = factory.Create();
            var resFile = uow.RawFileRepository.Find(addedFiles[1].Id);
            for (var i = 0; i < resFile.RawContent.Length; i++)
            {
                Assert.AreEqual(addedFiles[1].RawContent[i], resFile.RawContent[i]);
            }
        }

        [TestMethod]
        public void TestRemove()
        {
            var factory = new UnitOfWorkFactory(ApplicationSettings.GetInstance().DatabasePath);
            using var uow = factory.Create();
            var resFile = uow.RawFileRepository.Find(addedFiles[1].Id);

            Assert.IsNotNull(resFile);
            uow.RawFileRepository.Remove(resFile);
            Assert.AreEqual(0, resFile.Id);

            var resFile2 = uow.RawFileRepository.Find(addedFiles[1].Id);
            Assert.IsNull(resFile2);
        }
    }*/
}
