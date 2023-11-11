using System.IO;

namespace Valyreon.Elib.Wpf.Extensions
{
    public static class StringExtensions
    {
        public static bool Exists(this string filePath)
        {
            return File.Exists(filePath);
        }

        public static string GetExtension(this string filePath)
        {
            return Path.GetExtension(filePath);
        }

        public static bool IsDefined(this string str)
        {
            return !string.IsNullOrWhiteSpace(str);
        }

        public static bool IsFileLocked(this string filePath)
        {
            var fileInfo = new FileInfo(filePath);

            try
            {
                using var stream = fileInfo.Open(FileMode.Open, FileAccess.Read, FileShare.None);
                stream.Close();
            }
            catch (IOException)
            {
                //the file is unavailable because it is:
                //still being written to
                //or being processed by another thread
                //or does not exist (has already been processed)
                return true;
            }

            return false;
        }
    }
}
