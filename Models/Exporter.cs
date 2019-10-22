using DataLayer;
using Domain;
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

        public void ExportBook(Book book, EFile eFile, string destinationFolder)
        {
            byte[] binaryData = eFile.RawContent;
            StringBuilder fileNameBuilder = new StringBuilder();
            fileNameBuilder.Append(book.Name);

            if (book.SeriesId != null && book.Series == null) // If series was not included
            {
                database.Entry(book).Reference(b => b.Series).Load();
            }

            if (book.Authors == null) // if authors were not included
            {
                database.Entry(book).Reference(b => b.Authors).Load();
            }

            if (book.SeriesId != null)
            {
                fileNameBuilder.Append($" ({book.Series} #{book.NumberInSeries})");
            }

            // there can be more than one author
            int index = 1;
            fileNameBuilder.Append($" by {book.Authors.ElementAt(0).Name}");
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

            using FileStream fs = File.Create(Path.Combine(destinationFolder, fileNameBuilder.ToString()));
            fs.Write(binaryData, 0, binaryData.Length);
        }
    }
}