﻿using DataLayer;
using Domain;
using EbookTools;
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
            string bookFilePath = @"C:\Users\luka.budrak\Downloads\[Peter_Hollins]_Finish_What_You_Start__The_Art_of_(z-lib.org).epub";
            string[] collectionTags = new string[]
            {
                "fantasy",
                "adventure",
            };

            using ElibContext context = new ElibContext(ApplicationSettings.GetInstance().DatabasePath);
            // context.TruncateDatabase();

            var parsedBook = EbookParserFactory.Create(bookFilePath).Parse();
            Book newBook = new Book
            {
                Title = parsedBook.Title,
                Authors = new List<Author> { context.Authors.Where(au => au.Name.Equals(parsedBook.Author)).FirstOrDefault() ?? new Author() { Name = parsedBook.Author } },
                Cover = ImageOptimizer.ResizeAndFill(parsedBook.Cover),
                Files = new List<EFile> { new EFile { Format = parsedBook.Format, RawContent = parsedBook.RawData } }
            };
            context.Books.Add(newBook);
            context.SaveChanges();
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

        [TestMethod]
        public void AddBookSeriesFromMyComputer()
        {
            string bookSeriesPath = @"F:\Documents\Ebooks\Book Series";
            using ElibContext context = new ElibContext(ApplicationSettings.GetInstance().DatabasePath);
            context.TruncateDatabase();

            foreach (var dirPath in Directory.GetDirectories(bookSeriesPath))
            {
                string[] splitDirName = Path.GetFileName(dirPath).Split(new string[] { " by " }, StringSplitOptions.None);
                string seriesName = splitDirName[0];

                void DirSearch(string sDir)
                {
                    try
                    {
                        foreach (string d in Directory.GetDirectories(sDir))
                        {
                            foreach (string f in Directory.GetFiles(d))
                            {
                                if (f.EndsWith(".epub"))
                                {
                                    var parsedBook = EbookParserFactory.Create(f).Parse();
                                    Book newBook = new Book
                                    {
                                        Title = parsedBook.Title,
                                        Authors = new List<Author> { context.Authors.Where(au => au.Name.Equals(parsedBook.Author)).FirstOrDefault() ?? new Author() { Name = parsedBook.Author } },
                                        Series = seriesName == null ? null : (context.Series.Where(x => x.Name == seriesName).FirstOrDefault() ?? new BookSeries { Name = seriesName }),
                                        Cover = parsedBook.Cover,
                                        Files = new List<EFile>
                                        {
                                            new EFile { Format = parsedBook.Format, RawContent = parsedBook.RawData }
                                        }
                                    };
                                    context.Books.Add(newBook);
                                    context.SaveChanges();
                                }
                            }
                            DirSearch(d);
                        }
                    }
                    catch (System.Exception excpt)
                    {
                        Console.WriteLine(excpt.Message);
                    }
                }

                DirSearch(dirPath);
            }
        }

        [TestMethod]
        public void OptimizeImages()
        {
            using ElibContext context = new ElibContext(ApplicationSettings.GetInstance().DatabasePath);

            foreach (var book in context.Books.Where(b => true))
            {
                if (book.Cover != null)
                {
                    book.Cover = ImageOptimizer.ResizeAndFill(book.Cover);
                }
            }

            context.SaveChanges();
        }

        [TestMethod]
        public void OptimizeOneSquareImageTest()
        {
            var imgbytes = File.ReadAllBytes("exportcover.jpg");
            var result = ImageOptimizer.ResizeAndFill(imgbytes);
            File.WriteAllBytes("fillAndResize.jpg", result);
        }

        [TestMethod]
        public void ExportCover()
        {
            using ElibContext context = new ElibContext(ApplicationSettings.GetInstance().DatabasePath);

            File.WriteAllBytes("exportcover.jpg", context.Books.Find(258).Cover);
        }
    }
}