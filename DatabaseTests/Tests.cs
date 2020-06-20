using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using DataLayer;
using DataLayer.Repositories;
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

            exp.ExportBooks(uow.BookRepository.All(), options, uow);
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

            using var context = ApplicationSettings.CreateUnitOfWork();
            for (int i = 0; i < 15; i++)
            {
                context.CollectionRepository.Add(new UserCollection { Tag = RandomString(5) });
            }

            context.Commit();
        }

        [TestMethod]
        public void AddBookSeriesFromMyComputer()
        {
            string bookSeriesPath = @"D:\Documents\Ebooks\Book Series";
            using var uow = ApplicationSettings.CreateUnitOfWork();
            uow.Truncate();
            uow.Commit();
            foreach (string dirPath in Directory.GetDirectories(bookSeriesPath))
            {
                var splitDirName = Path.GetFileName(dirPath).Split(new[] { " by " }, StringSplitOptions.None);
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
                                Book newBook = parsedBook.ToBook(uow);

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
                                        BookSeries series = new BookSeries { Name = seriesName };
                                        uow.SeriesRepository.Add(series);
                                        newBook.SeriesId = series.Id;
                                    }
                                    else
                                    {
                                        newBook.SeriesId = existingSeries.Id;
                                    }
                                }

                                uow.BookRepository.Add(newBook);

                                Author existingAuthor = uow.AuthorRepository.GetAuthorWithName(authorsName);
                                if (existingAuthor == null)
                                {
                                    Author newAuthor = new Author { Name = authorsName };
                                    uow.AuthorRepository.AddAuthorForBook(newAuthor, newBook.Id);
                                }
                                else
                                {
                                    uow.AuthorRepository.AddAuthorForBook(existingAuthor, newBook.Id);
                                }

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
            uow.Commit();
        }

        [TestMethod]
        public void OptimizeImages()
        {
            using UnitOfWork context = new UnitOfWork(ApplicationSettings.GetInstance().DatabasePath);

            foreach (Book book in context.BookRepository.All().Select(b => b.LoadMembers(context)))
            {
                if (book.Cover != null)
                {
                    book.Cover.Image = ImageOptimizer.ResizeAndFill(book.Cover.Image);
                }
            }

            context.Commit();
        }

        [TestMethod]
        public void OptimizeOneSquareImageTest()
        {
            var imgbytes = File.ReadAllBytes("exportcover.jpg");
            var result = ImageOptimizer.ResizeAndFill(imgbytes);
            File.WriteAllBytes("fillAndResize.jpg", result);
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