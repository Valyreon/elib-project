namespace EbookTools
{
    public class ParsedBook
    {
        public ParsedBook(string title, string author, string isbn, string publisher, byte[] cover, string format,
            byte[] rawData)
        {
            this.Title = title;
            this.Author = author;
            this.Isbn = isbn;
            this.Publisher = publisher;
            this.Cover = cover;
            this.Format = format;
            this.RawData = rawData;
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