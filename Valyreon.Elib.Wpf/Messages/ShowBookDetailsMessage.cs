using Valyreon.Elib.Domain;
using Valyreon.Elib.Mvvm.Messaging;

namespace Valyreon.Elib.Wpf.Messages
{
	public class ShowBookDetailsMessage : MessageBase
	{
		public ShowBookDetailsMessage(Book clickedBook)
		{
			Book = clickedBook;
		}

		public Book Book { get; }
	}
}
