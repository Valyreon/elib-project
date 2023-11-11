using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Valyreon.Elib.Domain;
using Valyreon.Elib.Wpf.Models.Options;

namespace Valyreon.Elib.Wpf.Models
{
    public class Exporter
    {
        public static string GenerateName(Book book)
        {
            var fileNameBuilder = new StringBuilder();
            // there can be more than one author
            var index = 1;
            fileNameBuilder.Append(book.Title)
                           .Append(book.SeriesId == null ? string.Empty : $"({book.Series.Name} #{book.NumberInSeries})")
                           .Append(" by ")
                           .Append(book.Authors.ElementAt(0).Name);
            while (index < book.Authors.Count)
            {
                fileNameBuilder.Append(index == book.Authors.Count - 1 ? " and" : ",");
                fileNameBuilder.Append(' ').Append(book.Authors.ElementAt(index).Name);
                ++index;
            }

            fileNameBuilder.Append(book.Format);
            var fileName = fileNameBuilder.ToString();

            return Path.GetInvalidFileNameChars().Aggregate(fileName, (current, invalid) => current.Replace(char.ToString(invalid), ""));
        }

        public void ExportBooks(IEnumerable<Book> books, ExporterOptions options, Action<string> progressSet = null)
        {
            books = books.Where(b => File.Exists(b.Path));

            void ExportAllInList(IEnumerable<Book> list, string outPath)
            {
                foreach (var book in list)
                {
                    progressSet?.Invoke(book.Title);

                    ExportBookToFolder(book, outPath);
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
            if (enumerable.Count == 0)
            {
                return;
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
                    var thisGroupsDestPath = Path.Combine(options.DestinationDirectory, $"{group.Key}");
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
                    var thisAuthorsDestPath = Path.Combine(options.DestinationDirectory, $"{authorGroup.Key}");
                    Directory.CreateDirectory(thisAuthorsDestPath);

                    // then we group one authors(that is combination of authors) by series
                    ProcessBySeries(authorGroup, thisAuthorsDestPath);
                }
            }
        }

        private void ExportBookToFolder(Book book, string destinationFolder)
        {
            var fileName = GenerateName(book);
            File.Copy(book.Path, Path.Combine(destinationFolder, fileName), true);
        }
    }
}
