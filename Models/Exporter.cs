using DataLayer;
using Domain;
using Models.Options;
using System;
using System.Collections;
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

        private void ExportFile(EFile eFile, string destinationFolder)
        {
            StringBuilder fileNameBuilder = new StringBuilder();

            // there can be more than one author
            int index = 1;
            fileNameBuilder.Append($"{eFile.Book.Name}{((eFile.Book.SeriesId == null) ?(""):($"({eFile.Book.Series.Name} #{eFile.Book.NumberInSeries})"))} by {eFile.Book.Authors.ElementAt(0).Name}");
            while (index < eFile.Book.Authors.Count)
            {
                if (index == eFile.Book.Authors.Count - 1)
                {
                    fileNameBuilder.Append(" and");
                }
                else
                {
                    fileNameBuilder.Append(",");
                }
                fileNameBuilder.Append($" {eFile.Book.Authors.ElementAt(index).Name}");
                ++index;
            }
            fileNameBuilder.Append(eFile.Format);

            using FileStream fs = File.Create(Path.Combine(destinationFolder, fileNameBuilder.ToString()));
            fs.Write(eFile.RawContent, 0, eFile.RawContent.Length);
        }

        public void ExportBooks(IEnumerable<EFile> files, ExporterOptions options)
        {
            // Load everything needed
            foreach (EFile file in files)
            {
                database.Entry(file).Reference(f => f.Book).Load();
                database.Entry(file.Book).Reference("Series").Load();
                database.Entry(file.Book).Collection("Authors").Load();
            }

            string destFolder = null;
            if (options.CreateNewDirectory && options.NewDirectoryName == null)
                throw new ArgumentException("If CreateNewDirectory option is true, NewDirectoryName can't be null.");
            if (options.CreateNewDirectory)
                destFolder = Path.Combine(options.DestinationDirectory, options.NewDirectoryName);
            else
                destFolder = options.DestinationDirectory;

            Directory.CreateDirectory(destFolder);

            // Split in group according to options
            if (!options.GroupByAuthor && !options.GroupBySeries)
            {
                foreach (var file in files)
                {
                    ExportFile(file, destFolder);
                }
            }
            else if(!options.GroupByAuthor && options.GroupBySeries)
            {
                var groups = files.GroupBy(f => f.Book.Series?.Name);

                foreach(var group in groups)
                {
                    // create directory for this series
                    var list = group.ToList();
                    string thisGroupsDestPath = null;
                    if(group.Key != null) // only put books that have series in a separate folder
                    {
                        thisGroupsDestPath = Path.Combine(destFolder, $"{group.Key} Series");
                        Directory.CreateDirectory(thisGroupsDestPath);
                    }
                    else
                    {
                        thisGroupsDestPath = destFolder;
                    }

                    foreach (var file in list)
                    {
                        ExportFile(file, thisGroupsDestPath);
                    }
                }
            }
            else if(options.GroupByAuthor && !options.GroupBySeries)
            {
                var groups = files.GroupBy(f => f.Book.Authors.Select(a => a.Name).Aggregate((i, j) => i + ", " + j));

                foreach (var group in groups)
                {
                    // create directory for this author
                    var list = group.ToList();
                    string thisGroupsDestPath = Path.Combine(destFolder, $"{group.Key}");
                    Directory.CreateDirectory(thisGroupsDestPath);

                    foreach (var file in list)
                    {
                        ExportFile(file, thisGroupsDestPath);
                    }
                }
            }
            else // both must be true
            {
                // first group by authors
                var authorGroups = files.GroupBy(f => f.Book.Authors.Select(a => a.Name).Aggregate((i, j) => i + ", " + j));
                foreach(var authorGroup in authorGroups)
                {
                    // create directory for this author
                    string thisAuthorsDestPath = Path.Combine(destFolder, $"{authorGroup.Key}");
                    Directory.CreateDirectory(thisAuthorsDestPath);

                    // then we group one authors(that is combination of authors) by series
                    var seriesGroups = authorGroup.GroupBy(f => f.Book.Series?.Name);
                    foreach(var seriesGroup in seriesGroups)
                    {
                        var seriesList = seriesGroup.ToList();
                        string thisSeriesDestPath = Path.Combine(thisAuthorsDestPath, $"{seriesGroup.Key}");

                        if (seriesGroup.Key != null) // only put books in series in a separate folder
                        {
                            thisSeriesDestPath = Path.Combine(thisAuthorsDestPath, $"{seriesGroup.Key} Series");
                            Directory.CreateDirectory(thisSeriesDestPath);
                        }
                        else
                        {
                            thisSeriesDestPath = thisAuthorsDestPath;
                        }

                        foreach (var file in seriesList)
                        {
                            ExportFile(file, thisSeriesDestPath);
                        }
                    }
                }
            }
        }
    }
}