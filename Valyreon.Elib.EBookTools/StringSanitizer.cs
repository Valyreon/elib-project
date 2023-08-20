using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace Valyreon.Elib.EBookTools
{
    public static class StringSanitizerExtensions
    {
        public static string Clean(this string str, bool clearNewLines = true, bool clearForPath = true)
        {
            if (str == null)
            {
                return null;
            }

            var result = str;

            if (clearForPath)
            {
                var invalidPathChars = Path.GetInvalidPathChars();
                result = new string(result.ToCharArray().Where(c => !invalidPathChars.Contains(c)).ToArray());
            }

            // clean possible html
            result = Regex.Replace(result, @"(<.*?>)", string.Empty);

            // decode html character codes
            result = HttpUtility.HtmlDecode(result);

            // replace underscores with spaces
            result = result.Replace('_', ' ');

            // clean weird characters
            //result = Regex.Replace(result, @"([^\d\w' \-\.,\n?!&#@*+%]+)", string.Empty);

            if (clearNewLines)
            {
                result = result.Replace('\n', ' ');
            }

            return result.Trim();
        }

        public static string Prettify(this string str, int? maxLength = null)
        {
            if (str == null)
            {
                return null;
            }

            var result = Regex.Replace(str, @"(\.|\?|!|:)([A-Z])", "$1 $2");

            result = Regex.Replace(result, @"SUMMARY\s*:", string.Empty).Trim();

            if (maxLength.HasValue && result.Length > maxLength.Value)
            {
                result = result[..maxLength.Value] + "...";
            }

            return result;
        }
    }
}
