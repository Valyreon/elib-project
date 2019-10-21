using DataLayer;
using Domain;
using Models.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    public class Exporter
    {
        private readonly ElibContext database;

        public Exporter()
        {
            database = new ElibContext(ApplicationSettings.GetInstance().DatabasePath);
        }

        public static void ExportBook(Book book, string path, ExporterOptions.Format format)
        {
            byte[] outputBinary = book.Files.Where(x => x.Format == ExporterOptions.GetFormat(format)).FirstOrDefault()?.RawContent;

            if (outputBinary == null)
                throw new FileNotFoundException();

            using (FileStream fs = File.Create(path))
            {
                fs.Write(outputBinary, 0, outputBinary.Length);
            }
        }
    }
}
