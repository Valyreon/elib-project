using System.Drawing;

namespace EbookTools
{
	public class ParsedBook
	{
		public string Title { get; private set; } = null;

		public string Author { get; private set; } = null;

		public string ISBN { get; private set; } = null;

		public string Publisher { get; private set; } = null;

		public string ContentHtml { get; private set; } = null;

		public Image Cover { get; private set; }

		public ParsedBook(string title, string author, string isbn, string publisher, string content, Image cover)
		{
			Title = title;
			Author = author;
			ISBN = isbn;
			Publisher = publisher;
			ContentHtml = content;
			Cover = cover;
		}

		public ParsedBook(string content)
		{
			ContentHtml = content;
		}
	}
}