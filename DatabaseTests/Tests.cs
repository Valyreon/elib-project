﻿using DataLayer;
using Domain;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Models;
using Models.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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
                @"F:\Documents\Ebooks\Miscellaneous\To-Read\Revelation Space by Alastair Reynolds.epub"
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
            context.TruncateDatabase();

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
            foreach (var collectionTag in collectionTags)
            {
                var collection = context.UserCollections.Where(col => col.Tag.Equals(collectionTag)).FirstOrDefault();
                newBook.UserCollections = new List<UserCollection>
                {
                    collection ?? new UserCollection { Tag = collectionTag }
                };
            }

            // Add files, check if they exists on given location
            foreach (var filePath in bookFilePaths)
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

        [TestMethod]
        public void ExporterTest()
        {
            using ElibContext context = new ElibContext(ApplicationSettings.GetInstance().DatabasePath);
            Exporter exp = new Exporter(context);

            ExporterOptions options = new ExporterOptions
            {
                GroupByAuthor = true,
                GroupBySeries = true,
                CreateNewDirectory = true,
                NewDirectoryName = "TESTEXPORTFOLDER",
                DestinationDirectory = @"C:\Users\Luka\Desktop"
            };

            exp.ExportBookFiles(context.BookFiles.ToList(), options);
        }

        [TestMethod]
        public void MoreCollections()
        {
            Random random = new Random();
            string RandomString(int length)
            {
                const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
                return new string(Enumerable.Repeat(chars, length)
                  .Select(s => s[random.Next(s.Length)]).ToArray());
            }

            using ElibContext context = new ElibContext(ApplicationSettings.GetInstance().DatabasePath);
            for (int i = 0; i < 15; i++)
            {
                context.UserCollections.Add(new UserCollection { Tag = RandomString(5) });
            }
            context.SaveChanges();
        }
    }
}