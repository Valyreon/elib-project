using Domain;
using MVVMLibrary.Messaging;

namespace ElibWpf.Messages
{
	public class AuthorSelectedMessage : MessageBase
	{
		public AuthorSelectedMessage(Author author)
		{
			Author = author;
		}

		public Author Author { get; }
	}
}
