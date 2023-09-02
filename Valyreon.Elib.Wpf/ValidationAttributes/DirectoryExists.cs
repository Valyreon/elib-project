using System.ComponentModel.DataAnnotations;
using System.IO;

namespace Valyreon.Elib.Wpf.ValidationAttributes
{
    public class DirectoryExists : ValidationAttribute
    {
        private readonly bool invert;

        public DirectoryExists()
        {
        }

        public DirectoryExists(bool invert)
        {
            this.invert = invert;
        }

        public override bool IsValid(object value)
        {
            var strValue = value as string;

            if (value is null)
            {
                return true;
            }

            var res = Directory.Exists(strValue);

            return invert ? !res : res;
        }
    }
}
