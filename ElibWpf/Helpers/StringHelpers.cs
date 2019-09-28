using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElibWpf.Helpers
{
    public static class StringHelpers
    {
        public static Tuple<string, string> SplitOnFirstBlank(this string str)
        {
            if (string.IsNullOrEmpty(str))
                return new Tuple<string, string>(string.Empty, string.Empty);

            string[] parts = str.Split(new[] { ' ' }, 2);

            return parts.Length == 2 ? new Tuple<string, string>(parts[0], parts[1]) : new Tuple<string, string>(parts[0], string.Empty);
        }
    }
}
