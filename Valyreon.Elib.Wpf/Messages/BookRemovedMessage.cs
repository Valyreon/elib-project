using System.Collections.Generic;
using Valyreon.Elib.Domain;
using Valyreon.Elib.Mvvm.Messaging;

namespace Valyreon.Elib.Wpf.Messages
{
    public class BooksRemovedMessage : MessageBase
    {
        public BooksRemovedMessage(IEnumerable<Book> books)
        {
            Books = books;
        }

        public IEnumerable<Book> Books { get; }
    }
}
