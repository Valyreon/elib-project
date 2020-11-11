using DataLayer;
using Domain;
using ElibWpf.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Text;

namespace DatabaseTests.RepositoryTests
{
	[TestClass]
	public class RawFileRepositoryTests
	{
		private List<RawFile> addedFiles;

		[TestInitialize]
		public void Initialize()
		{
			using var unitOfWork = new UnitOfWork(ApplicationSettings.GetInstance().DatabasePath);

			addedFiles = new List<RawFile>
			{
				new RawFile { RawContent = Encoding.ASCII.GetBytes("One File") },
				new RawFile { RawContent = Encoding.ASCII.GetBytes("Two File") },
				new RawFile { RawContent = Encoding.ASCII.GetBytes("Three File") }
			};

			foreach(var col in addedFiles)
			{
				unitOfWork.RawFileRepository.Add(col);
			}

			unitOfWork.Commit();
		}

		[TestCleanup]
		public void Clean()
		{
			foreach(var file in addedFiles)
			{
				using var unitOfWork = new UnitOfWork(ApplicationSettings.GetInstance().DatabasePath);
				unitOfWork.RawFileRepository.Remove(file.Id);
				unitOfWork.Commit();
			}
		}

		[TestMethod]
		public void TestFind()
		{
			using var uow = new UnitOfWork(ApplicationSettings.GetInstance().DatabasePath);
			var resFile = uow.RawFileRepository.Find(addedFiles[1].Id);
			for(var i = 0 ; i < resFile.RawContent.Length ; i++)
			{
				Assert.AreEqual(addedFiles[1].RawContent[i], resFile.RawContent[i]);
			}
		}

		[TestMethod]
		public void TestRemove()
		{
			using var uow = new UnitOfWork(ApplicationSettings.GetInstance().DatabasePath);
			var resFile = uow.RawFileRepository.Find(addedFiles[1].Id);

			Assert.IsNotNull(resFile);
			uow.RawFileRepository.Remove(resFile);
			Assert.AreEqual(0, resFile.Id);

			var resFile2 = uow.RawFileRepository.Find(addedFiles[1].Id);
			Assert.IsNull(resFile2);
		}
	}
}
