using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DataLayer;
using Domain;
using EbookTools;
using ElibWpf.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Models;
using Models.Options;

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
                .Include("File")
                .Include("Quotes")
                .FirstOrDefault();
            Assert.IsNotNull(first?.Books);
            Assert.IsNotNull(collection?.Books);
            Assert.IsNotNull(mybook?.Series);
            Assert.IsNotNull(mybook.Authors);
            Assert.IsNotNull(mybook.File);
            Assert.IsNotNull(mybook.Quotes);
        }

        /*[TestMethod]
        public void TestAddingToDatabase()
        {
            const string bookFilePath = @"C:\Users\luka.budrak\Downloads\[Peter_Hollins]_Finish_What_You_Start__The_Art_of_(z-lib.org).epub";
            string[] collectionTags =
            {
                "fantasy",
                "adventure"
            };

            using ElibContext context = new ElibContext(ApplicationSettings.GetInstance().DatabasePath);
            // context.TruncateDatabase();

            ParsedBook parsedBook = EbookParserFactory.Create(bookFilePath).Parse();
            Book newBook = new Book
            {
                Title = parsedBook.Title,
                Authors = new List<Author>
                {
                    context.Authors.FirstOrDefault(au => au.Name.Equals(parsedBook.Author)) ??
                    new Author {Name = parsedBook.Author}
                },
                Cover = ImageOptimizer.ResizeAndFill(parsedBook.Cover),
                File = new EFile
                {
                    Format = parsedBook.Format,
                    Signature = Signer.ComputeHash(parsedBook.RawData),
                    RawFile = new RawFile {RawContent = parsedBook.RawData}
                }
            };
            context.Books.Add(newBook);
            context.SaveChanges();
        }*/

        [TestMethod]
        public void ExporterTest()
        {
            using var uow = ApplicationSettings.CreateUnitOfWork();
            Exporter exp = new Exporter(uow);

            ExporterOptions options = new ExporterOptions
            {
                GroupByAuthor = true,
                GroupBySeries = true,
                DestinationDirectory = @"C:\Users\Luka\Desktop"
            };

            exp.ExportBooks(uow.BookRepository.All(), options);
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
                context.UserCollections.Add(new UserCollection {Tag = RandomString(5)});
            }

            context.SaveChanges();
        }

        [TestMethod]
        public void AddBookSeriesFromMyComputer()
        {
            /*string bookSeriesPath = @"D:\Documents\Ebooks\Book Series";
            using ElibContext context = new ElibContext(ApplicationSettings.GetInstance().DatabasePath);
            context.TruncateDatabase();

            foreach (string dirPath in Directory.GetDirectories(bookSeriesPath))
            {
                var splitDirName = Path.GetFileName(dirPath).Split(new[] {" by "}, StringSplitOptions.None);
                string seriesName = splitDirName[0];
                string authorsName = splitDirName[1];

                void DirSearch(string sDir)
                {
                    try
                    {
                        foreach (string d in Directory.GetDirectories(sDir))
                        {
                            foreach (string f in Directory.GetFiles(d))
                            {
                                if (!f.EndsWith(".epub"))
                                {
                                    continue;
                                }

                                ParsedBook parsedBook = EbookParserFactory.Create(f).Parse();
                                Book newBook = parsedBook.ToBook();
                                newBook.Series = seriesName == null
                                    ? null
                                    : context.Series.FirstOrDefault(x => x.Name == seriesName) ??
                                      new BookSeries {Name = seriesName};

                                Author existingAuthor = context.Authors.FirstOrDefault(c => c.Name == authorsName);
                                if (existingAuthor == null)
                                {
                                    Author newAuthor = new Author
                                    {
                                        Name = authorsName
                                    };
                                    newBook.Authors = new List<Author> {newAuthor};
                                }
                                else
                                {
                                    newBook.Authors = new List<Author> {existingAuthor};
                                }

                                context.Books.Add(newBook);
                                context.SaveChanges();
                            }

                            DirSearch(d);
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.StackTrace);
                        Console.WriteLine(e.Message);
                    }
                }

                DirSearch(dirPath);
            }*/
        }

        [TestMethod]
        public void OptimizeImages()
        {
            using ElibContext context = new ElibContext(ApplicationSettings.GetInstance().DatabasePath);

            foreach (Book book in context.Books.Where(b => true))
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

            File.WriteAllBytes("exportcover.jpg", context.Books.Find(258)?.Cover ?? throw new InvalidOperationException());
        }

        [TestMethod]
        public void TestHash()
        {
            string path =
                @"D:\Documents\Ebooks\Miscellaneous\The Farseer Trilogy by Robin Hobb\[Robin_Hobb]_Assassin's_Apprentice_(The_Farseer_Tr(zlibraryexau2g3p.onion).epub";

            Signer.ComputeHash(File.ReadAllBytes(path));
        }
    }
}