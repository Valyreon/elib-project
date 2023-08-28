using Valyreon.Elib.Domain;
using Valyreon.Elib.Mvvm.Messaging;

namespace Valyreon.Elib.Wpf.Messages
{
    public class BookRemovedMessage : MessageBase
    {
        public BookRemovedMessage(Book book)
        {
            Book = book;
        }

        public Book Book { get; }
    }
}
