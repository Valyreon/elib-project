using System.Collections.Generic;
using Valyreon.Elib.Domain;
using Valyreon.Elib.Mvvm.Messaging;

namespace Valyreon.Elib.Wpf.Messages
{
    public class OpenBookDetailsFlyoutMessage : MessageBase
    {
        public OpenBookDetailsFlyoutMessage(LinkedListNode<Book> book)
        {
            Book = book;
        }

        public LinkedListNode<Book> Book { get; }
    }
}
