using System.Collections.Generic;

namespace Valyreon.Elib.EBookTools
{
    public class ParsedBook
    {
        public List<string> Authors { get; set; }

        public byte[] Cover { get; set; }

        public string Path { get; set; }

        public string Isbn { get; set; }

        public string Publisher { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }
    }
}
