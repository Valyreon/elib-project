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

        public void ExportBook(EFile eFile, string path)
        {
            byte[] binaryData = eFile.RawContent;

            using(FileStream fs = File.Create(path))
            {
                fs.Write(binaryData, 0, binaryData.Length);
            }
        }
    }
}
