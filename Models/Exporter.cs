using DataLayer;
using Domain;
using Models.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Models
{
    public class Exporter
    {
        private readonly ElibContext database;

        public Exporter(ElibContext db)
        {
            this.database = db;
        }

        private void ExportBook(Book book, string destinationFolder)
        {
            StringBuilder fileNameBuilder = new StringBuilder();

            // there can be more than one author
            int index = 1;
            fileNameBuilder.Append($"{book.Title}{((book.SeriesId == null) ? ("") : ($"({book.Series.Name} #{book.NumberInSeries})"))} by {book.Authors.ElementAt(0).Name}");
            while (index < book.Authors.Count)
            {
                if (index == book.Authors.Count - 1)
                {
                    fileNameBuilder.Append(" and");
                }
                else
                {
                    fileNameBuilder.Append(",");
                }
                fileNameBuilder.Append($" {book.Authors.ElementAt(index).Name}");
                ++index;
            }
            fileNameBuilder.Append(book.File.Format);
            string fileName = fileNameBuilder.ToString();

            foreach(char invalid in Path.GetInvalidFileNameChars())
            {
                fileName = fileName.Replace(char.ToString(invalid), "");
            }

            using FileStream fs = File.Create(Path.Combine(destinationFolder, fileName));
            fs.Write(book.File.RawFile.RawContent, 0, book.File.RawFile.RawContent.Length);
        }

        public void ExportBooks(IEnumerable<Book> books, ExporterOptions options, Action<string> progressSet = null)
        {
            void ExportAllInList(IEnumerable<Book> list, string outPath)
            {
                foreach (var book in list)
                {
                    progressSet(book.Title);
                    ExportBook(book, outPath);
                }
            };
            void ProcessBySeries(IEnumerable<Book> bookList, string outPath)
            {
                var groups = bookList.GroupBy(book => book.Series?.Id);
                foreach (var group in groups)
                {
                    // create directory for this series
                    string thisGroupsDestPath = null;
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
                throw new ArgumentNullException();
            else if (books.Count() == 0)
                return;

            // Load everything needed
            foreach (var book in books)
            {
                database.Entry(book).Reference(b => b.File).Load();
                database.Entry(book.File).Reference(f => f.RawFile).Load();
                database.Entry(book).Reference(b => b.Series).Load();
                database.Entry(book).Collection(b => b.Authors).Load();
            }

            Directory.CreateDirectory(options.DestinationDirectory);

            // Split in groups according to options
            if (!options.GroupByAuthor && !options.GroupBySeries)
            {
                ExportAllInList(books, options.DestinationDirectory);
            }
            else if (!options.GroupByAuthor && options.GroupBySeries)
            {
                ProcessBySeries(books, options.DestinationDirectory);
            }
            else if (options.GroupByAuthor && !options.GroupBySeries)
            {
                var groups = books.GroupBy(b => b.Authors.Select(a => a.Name).Aggregate((i, j) => i + ", " + j));

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
                var authorGroups = books.GroupBy(book => book.Authors.Select(a => a.Name).Aggregate((i, j) => i + ", " + j));
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