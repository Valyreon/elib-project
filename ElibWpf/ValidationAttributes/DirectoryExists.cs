using System.ComponentModel.DataAnnotations;
using System.IO;

namespace ElibWpf.ValidationAttributes
{
    public class DirectoryExists : ValidationAttribute
    {
        private readonly bool invert;

        public DirectoryExists() { }

        public DirectoryExists(bool invert)
        {
            this.invert = invert;
        }

        public override bool IsValid(object value)
        {
            string strValue = value as string;

            if (value is null)
            {
                return true;
            }

            bool res = Directory.Exists(strValue);

            if (this.invert)
            {
                return !res;
            }

            return res;
        }
    }
}