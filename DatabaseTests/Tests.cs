using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DataLayer;
using Domain;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Models;

namespace DatabaseTests
{
    [TestClass]
    public class Tests
    {
        [TestMethod]
        public void TestRetrieval()
        {
            using ElibContext context = new ElibContext(ApplicationSettings.GetInstance().DatabasePath);

            // EntityFramework WILL NOT get items from other tables if not specifically asked to do so
            // If you want it to pull something as a graph, you need to use Include("NameOfPropertyToInclude");
            // See examples:
            Author first = context.Authors.Include("Books").FirstOrDefault();
            UserCollection collection = context.UserCollections.Include("Books").FirstOrDefault();
            Book mybook = context.Books
                .Include("Series")
                .Include("Authors")
                .Include("Files")
                .Include("Quotes")
                .Where(x => x.Name.Contains("Conversation"))
                .FirstOrDefault();
            Assert.IsNotNull(first.Books); 
            Assert.IsNotNull(collection.Books);
            Assert.IsNotNull(mybook.Series);
            Assert.IsNotNull(mybook.Authors);
            Assert.IsNotNull(mybook.Files);
            Assert.IsNotNull(mybook.Quotes);
        }

        [TestMethod]
        public void TestAddingToDatabase()
        {
            string[] bookFilePaths = new string[] 
            {   
                @"C:\Users\luka.budrak\Downloads\[Reynolds_Alastair]_Revelation_Space(z-lib.org).mobi"
            };
            string coverPicturePath = null;
            string[] authorNames = new string[] 
            {
                "Alastair Reynolds"
            };
            string bookName = "Revelation Space";
            string seriesName = "Revelation Space";
            string[] collectionTags = new string[] 
            {
                "scifi",
                "adventure"
            };

            using ElibContext context = new ElibContext(ApplicationSettings.GetInstance().DatabasePath);

            // Create new book object
            Book newBook = new Book
            {
                Name = bookName,
                Series = seriesName != null ? new BookSeries
                {
                    Name = seriesName,
                } : null,
                NumberInSeries = 1,
                IsRead = true,
                Cover = coverPicturePath == null ? null : File.ReadAllBytes(coverPicturePath)
            };

            // Add authors, but first check if each exists in database
            foreach (var authorName in authorNames)
            {
                var author = context.Authors.Where(au => au.Name.Equals("")).FirstOrDefault();
                newBook.Authors = new List<Author>
                {
                    author ?? new Author() { Name = authorName }
                };
            }

            // Add collections, but first check if each exists in database
            foreach(var collectionTag in collectionTags)
            {
                var collection = context.UserCollections.Where(col => col.Tag.Equals(collectionTag)).FirstOrDefault();
                newBook.UserCollections = new List<UserCollection>
                {
                    collection ?? new UserCollection { Tag = collectionTag }
                };
            }

            // Add files, check if they exists on given location
            foreach(var filePath in bookFilePaths)
            {
                if (File.Exists(filePath))
                {
                    newBook.Files = new List<EFile>
                    {
                        new EFile {
                            RawContent = File.ReadAllBytes(filePath),
                            Format = Path.GetExtension(filePath)
                        }
                    };
                }
            }

            // Now add book to database and commit changes
            context.Books.Add(newBook);
            context.SaveChanges();

            // That is it, Entity Framework will take care of all the mapping and many to many relationships.
        }
    }
}
