﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Models.Helpers
{
    // Utility classes are useful by itself; whereas helper classes are classes with extension methods which will help extend the types.
    public static class StringHelpers
    {
        public static Tuple<string, string> SplitOnFirstBlank(this string str)
        {
            if (string.IsNullOrEmpty(str))
                return new Tuple<string, string>(string.Empty, string.Empty);

            string[] parts = str.Split(new[] { ' ' }, 2);

            return parts.Length == 2 ? new Tuple<string, string>(parts[0], parts[1]) : new Tuple<string, string>(parts[0], string.Empty);
        }

        /// <summary>
        /// Extracts paths from a string seperated by a space or enclosed in quotations
        /// Input example: \path\fileA \path\fileB "\path\file C\"
        /// </summary>
        /// <param name="str">String containing one or more paths</param>
        /// <returns>IEnumberable of paths</returns>
        public static IEnumerable<string> GetFilePathsFromString(this string str)
        {
            Regex splitRegex = new Regex("(?<=\")[^\"]*(?=\")|[^\" ]+");
            return splitRegex.Matches(str).Cast<Match>().Select(x => x.Value);
        }
    }
}