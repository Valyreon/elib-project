using Valyreon.Elib.Domain;
using Valyreon.Elib.Mvvm.Messaging;
using System.Collections.Generic;

namespace Valyreon.Elib.Wpf.Messages
{
	public class OpenAddBooksFormMessage : MessageBase
	{
		public OpenAddBooksFormMessage(IList<Book> booksToAdd)
		{
			BooksToAdd = booksToAdd;
		}

		public IList<Book> BooksToAdd { get; }
	}
}
