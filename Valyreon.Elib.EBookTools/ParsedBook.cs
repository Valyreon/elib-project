namespace Valyreon.Elib.EBookTools
{
    public class ParsedBook
    {
        public ParsedBook(string title, string author, string isbn, string publisher, byte[] cover, string format,
            byte[] rawData)
        {
            Title = title;
            Author = author;
            Isbn = isbn;
            Publisher = publisher;
            Cover = cover;
            Format = format;
            RawData = rawData;
        }

        public string Author { get; }

        public byte[] Cover { get; }

        public string Format { get; }

        public string Isbn { get; }

        public string Publisher { get; }

        public byte[] RawData { get; }

        public string Title { get; }
    }
}
