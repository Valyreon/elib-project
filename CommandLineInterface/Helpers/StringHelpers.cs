using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Models.Helpers
{
    // Utility classes are useful by itself; whereas helper classes are classes with extension methods which will help extend the types.
    public static class StringHelpers
    {
        public static Tuple<string, string> SplitOnFirstBlank(this string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return new Tuple<string, string>(string.Empty, string.Empty);
            }

            var parts = str.Split(new[] {' '}, 2);

            return parts.Length == 2
                ? new Tuple<string, string>(parts[0], parts[1])
                : new Tuple<string, string>(parts[0], string.Empty);
        }

        /// <summary>
        ///     Extracts paths from a string seperated by a space or enclosed in quotations
        ///     Input example: \path\fileA \path\fileB "\path\file C\"
        /// </summary>
        /// <param name="str">String containing one or more paths</param>
        /// <returns>IEnumerable of paths</returns>
        public static IEnumerable<string> GetFilePathsFromString(this string str)
        {
            Regex splitRegex = new Regex("(?<=\")[^\"]*(?=\")|[^\" ]+");
            return splitRegex.Matches(str).Cast<Match>().Select(x => x.Value);
        }

        public static ISet<int> GetIDsSeperatedBySpace(this string str)
        {
            ISet<int> result = new HashSet<int>();

            foreach (string x in str.Split(' '))
            {
                try
                {
                    result.Add(int.Parse(x));
                }
                catch (FormatException) { }
            }

            return result;
        }
    }
}