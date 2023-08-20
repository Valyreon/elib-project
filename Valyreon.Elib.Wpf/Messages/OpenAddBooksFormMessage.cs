using System.Collections.Generic;
using Valyreon.Elib.Mvvm.Messaging;

namespace Valyreon.Elib.Wpf.Messages
{
    public class OpenAddBooksFormMessage : MessageBase
	{
		public OpenAddBooksFormMessage(IList<string> booksToAdd)
		{
			BooksToAdd = booksToAdd;
		}

		public IList<string> BooksToAdd { get; }
	}
}
