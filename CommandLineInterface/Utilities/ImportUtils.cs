using System.Collections.Generic;
using System.IO;
using System.Linq;
using EbookTools;

namespace Models.Utilities
{
    // Utility classes are useful by itself; whereas helper classes are classes with extension methods which will help extend the types.
    public class ImportUtils
    {
        public static bool IsValidBookExtension(string path)
        {
            foreach (string extension in EbookParserFactory.SupportedExtensions)
            {
                if (Path.GetExtension(path) == extension)
                {
                    return true;
                }
            }

            return false;
        }

        public static IEnumerable<string> GetValidBookPaths(IEnumerable<string> fileList)
        {
            return fileList.Where(filePath => IsValidBookExtension(filePath) && File.Exists(filePath));
        }

        public static IList<string> GetValidFilesFromDirectory(string directory)
        {
            if (!Directory.Exists(directory))
            {
                throw new DirectoryNotFoundException();
            }

            var validFileList = new List<string>();

            foreach (string ext in EbookParserFactory.SupportedExtensions)
            {
                validFileList.AddRange(Directory.GetFiles(directory, "*" + ext));
            }

            return validFileList;
        }

        public static IEnumerable<string> GetInvalidBookPaths(IEnumerable<string> fileList)
        {
            return fileList.Where(filePath => !IsValidBookExtension(filePath) || !File.Exists(filePath));
        }

        public static IEnumerable<ParsedBook> GetParsedBooksFromPaths(IEnumerable<string> fileList)
        {
            IList<ParsedBook> result = new List<ParsedBook>();
            foreach (string file in fileList)
            {
                result.Add(EbookParserFactory.Create(file).Parse());
            }

            return result;
        }
    }
}