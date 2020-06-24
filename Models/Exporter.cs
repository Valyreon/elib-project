using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using DataLayer;
using Domain;
using Models.Options;

namespace Models
{
    public class Exporter
    {
        private readonly IUnitOfWork database;

        public Exporter(IUnitOfWork db)
        {
            this.database = db;
        }

        public static string GenerateName(Book book)
        {
            StringBuilder fileNameBuilder = new StringBuilder();
            // there can be more than one author
            int index = 1;
            fileNameBuilder.Append(
                $"{book.Title}{(book.SeriesId == null ? "" : $"({book.Series.Name} #{book.NumberInSeries})")} by {book.Authors.ElementAt(0).Name}");
            while (index < book.Authors.Count)
            {
                fileNameBuilder.Append(index == book.Authors.Count - 1 ? " and" : ",");
                fileNameBuilder.Append($" {book.Authors.ElementAt(index).Name}");
                ++index;
            }

            fileNameBuilder.Append(book.File.Format);
            string fileName = fileNameBuilder.ToString();

            fileName = Path.GetInvalidFileNameChars().Aggregate(fileName, (current, invalid) => current.Replace(char.ToString(invalid), ""));

            return fileName;
        }

        public void Export(RawFile file, string filePath)
        {
            using FileStream fs = File.Create(filePath);
            fs.Write(file.RawContent, 0, file.RawContent.Length);
        }

        private void ExportBookToFolder(Book book, string destinationFolder)
        {
            string fileName = GenerateName(book);
            using FileStream fs = File.Create(Path.Combine(destinationFolder, fileName));
            fs.Write(book.File.RawFile.RawContent, 0, book.File.RawFile.RawContent.Length);
        }

        public void ExportBooks(IEnumerable<Book> books, ExporterOptions options, IUnitOfWork uow, Action<string> progressSet = null)
        {
            void ExportAllInList(IEnumerable<Book> list, string outPath)
            {
                foreach (Book book in list)
                {
                    progressSet?.Invoke(book.Title);

                    this.ExportBookToFolder(book, outPath);
                }
            }

            void ProcessBySeries(IEnumerable<Book> bookList, string outPath)
            {
                var groups = bookList.GroupBy(book => book.Series?.Name);
                foreach (var group in groups)
                {
                    // create directory for this series
                    string thisGroupsDestPath;
                    if (group.Key != null) // only put books that have series in a separate folder
                    {
                        thisGroupsDestPath = Path.Combine(outPath, $"{group.Key} Series");
                        Directory.CreateDirectory(thisGroupsDestPath);
                    }
                    else
                    {
                        thisGroupsDestPath = outPath;
                    }

                    ExportAllInList(group, thisGroupsDestPath);
                }
            }

            if (books == null)
            {
                throw new ArgumentNullException();
            }

            var enumerable = books.ToList();
            if (!enumerable.Any())
            {
                return;
            }

            // Load everything needed
            foreach (Book book in enumerable)
            {
                book.File.RawFile = uow.RawFileRepository.Find(book.File.Id);
            }

            Directory.CreateDirectory(options.DestinationDirectory);

            // Split in groups according to options
            if (!options.GroupByAuthor && !options.GroupBySeries)
            {
                ExportAllInList(enumerable, options.DestinationDirectory);
            }
            else if (!options.GroupByAuthor && options.GroupBySeries)
            {
                ProcessBySeries(enumerable, options.DestinationDirectory);
            }
            else if (options.GroupByAuthor && !options.GroupBySeries)
            {
                var groups = enumerable.GroupBy(b => b.Authors.Select(a => a.Name).Aggregate((i, j) => i + ", " + j));

                foreach (var group in groups)
                {
                    // create directory for this author
                    string thisGroupsDestPath = Path.Combine(options.DestinationDirectory, $"{group.Key}");
                    Directory.CreateDirectory(thisGroupsDestPath);
                    ExportAllInList(group, thisGroupsDestPath);
                }
            }
            else // both must be true
            {
                // first group by authors
                var authorGroups =
                    enumerable.GroupBy(book => book.Authors.Select(a => a.Name).Aggregate((i, j) => i + ", " + j));
                foreach (var authorGroup in authorGroups)
                {
                    // create directory for this author
                    string thisAuthorsDestPath = Path.Combine(options.DestinationDirectory, $"{authorGroup.Key}");
                    Directory.CreateDirectory(thisAuthorsDestPath);

                    // then we group one authors(that is combination of authors) by series
                    ProcessBySeries(authorGroup, thisAuthorsDestPath);
                }
            }
        }
    }
}