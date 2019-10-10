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
                @"F:\Documents\Ebooks\Miscellaneous\Read\Papillon by Henri Charriere.epub"
            };
            string coverPicturePath = @"C:\Users\Luka\Desktop\9139723842_25ef6557c3_b.jpg";
            string[] authorNames = new string[] 
            {
                "Henri Charriere"
            };
            string bookName = "Papillon";
            string seriesName = "Papillon";
            string[] collectionTags = new string[] 
            {
                "adventure"
            };

            using ElibContext context = new ElibContext(ApplicationSettings.GetInstance().DatabasePath);

            // Create new book object
            Book newBook = new Book
            {
                Name = bookName,
                Series = new BookSeries
                {
                    Name = seriesName,
                },
                IsRead = true,
                Cover = File.ReadAllBytes(coverPicturePath)
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
