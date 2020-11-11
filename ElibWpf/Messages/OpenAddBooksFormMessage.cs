﻿using Domain;
using MVVMLibrary.Messaging;
using System.Collections.Generic;

namespace ElibWpf.Messages
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
