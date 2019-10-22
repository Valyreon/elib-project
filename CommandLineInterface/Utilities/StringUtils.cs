using System;

namespace CommandLineInterface.Utilities
{
    public class StringUtils
    {
        public static bool EqualsIgnoreCase(string a, string b)
        {
            return string.Equals(a, b, StringComparison.InvariantCultureIgnoreCase);
        }
    }
}