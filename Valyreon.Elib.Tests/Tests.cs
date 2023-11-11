namespace Valyreon.Elib.Tests
{
    /*[TestClass]
    public class Tests
    {
        [TestMethod]
        public void ExporterTest()
        {
            var factory = new UnitOfWorkFactory(ApplicationSettings.GetInstance().DatabasePath);
            using var uow = factory.Create();
            var exp = new Exporter(uow);

            var options = new ExporterOptions
            {
                GroupByAuthor = true,
                GroupBySeries = true,
                DestinationDirectory = @"C:\Users\Luka\Desktop"
            };

            exp.ExportBooks(uow.BookRepository.All(), options);
        }

        [TestMethod]
        public void BookToHtml()
        {
            const string inputPath = @"D:\Documents\Ebooks\Miscellaneous\I Will Teach You to Be Rich, Second Edition No Guilt. No Excuses. No B.S. Just a 6-Week Program That Works. by Ramit Sethi (z-lib.org).epub";
            const string outputPath = @"C:\Users\Luka\Desktop\IWillTeachYouToBeRich.html";

            var html = EbookParserFactory.Create(inputPath).GenerateHtml();
            File.WriteAllText(outputPath, html);
        }

        [TestMethod]
        public void MoreCollections()
        {
            var random = new Random();

            string RandomString(int length)
            {
                const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
                return new string(Enumerable.Repeat(chars, length)
                    .Select(s => s[random.Next(s.Length)]).ToArray());
            }

            var factory = new UnitOfWorkFactory(ApplicationSettings.GetInstance().DatabasePath);
            using var uow = factory.Create();
            for (var i = 0; i < 15; i++)
            {
                uow.CollectionRepository.Add(new UserCollection { Tag = RandomString(5) });
            }

            uow.Commit();
        }

        [TestMethod]
        public void AddBookSeriesFromMyComputer()
        {
            const string bookSeriesPath = @"D:\Documents\Ebooks\Book Series";
            var factory = new UnitOfWorkFactory(ApplicationSettings.GetInstance().DatabasePath);
            using var uow = factory.Create();
            uow.Truncate();
            uow.Vacuum();
            uow.Commit();

            foreach (var dirPath in Directory.GetDirectories(bookSeriesPath))
            {
                var splitDirName = Path.GetFileName(dirPath).Split(new[] { " by " }, StringSplitOptions.None);
                var seriesName = splitDirName[0];
                var authorsName = splitDirName[1];

                void DirSearch(string sDir)
                {
                    try
                    {
                        foreach (var d in Directory.GetDirectories(sDir))
                        {
                            foreach (var f in Directory.GetFiles(d))
                            {
                                if (!f.EndsWith(".epub"))
                                {
                                    continue;
                                }

                                var parsedBook = EbookParserFactory.Create(f).Parse();
                                var newBook = parsedBook.ToBook(uow);

                                if (newBook.Cover != null)
                                {
                                    uow.CoverRepository.Add(newBook.Cover);
                                    newBook.CoverId = newBook.Cover.Id;
                                }

                                uow.RawFileRepository.Add(newBook.File.RawFile);
                                newBook.File.RawFileId = newBook.File.RawFile.Id;
                                uow.EFileRepository.Add(newBook.File);
                                newBook.FileId = newBook.File.Id;

                                if (seriesName != null)
                                {
                                    var existingSeries = uow.SeriesRepository.GetByName(seriesName);
                                    if (existingSeries == null)
                                    {
                                        var series = new BookSeries { Name = seriesName };
                                        uow.SeriesRepository.Add(series);
                                        newBook.SeriesId = series.Id;
                                    }
                                    else
                                    {
                                        newBook.SeriesId = existingSeries.Id;
                                    }
                                }

                                uow.BookRepository.Add(newBook);

                                var existingAuthor = uow.AuthorRepository.GetAuthorWithName(authorsName);
                                if (existingAuthor == null)
                                {
                                    var newAuthor = new Author { Name = authorsName };
                                    uow.AuthorRepository.AddAuthorForBook(newAuthor, newBook.Id);
                                }
                                else
                                {
                                    uow.AuthorRepository.AddAuthorForBook(existingAuthor, newBook.Id);
                                }
                                uow.Commit();
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
            }
        }

        [TestMethod]
        public void OptimizeImageTest()
        {
            const string outputPath = @"C:\Users\Luka\Desktop\edited.jpg";
            const string inputPath = @"C:\Users\Luka\Desktop\The-Crying-Book-by-Heather-Christie-1.jpg";
            File.WriteAllBytes(outputPath, ImageOptimizer.ResizeAndFill(File.ReadAllBytes(inputPath)));
        }
    }*/
}
