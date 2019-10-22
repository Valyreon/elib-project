namespace EbookTools
{
    public class ParsedBook
    {
        public string Title { get; private set; } = null;

        public string Author { get; private set; } = null;

        public string ISBN { get; private set; } = null;

        public string Publisher { get; private set; } = null;

        public string ContentHtml { get; private set; } = null;

        public string Format { get; private set; } = null;

        public byte[] RawData { get; private set; } = null;

        public byte[] Cover { get; private set; }

        public ParsedBook(string title, string author, string isbn, string publisher, string content, byte[] cover, string format, byte[] rawData)
        {
            this.Title = title;
            this.Author = author;
            this.ISBN = isbn;
            this.Publisher = publisher;
            this.ContentHtml = content;
            this.Cover = cover;
            this.Format = format;
            this.RawData = rawData;
        }
    }
}