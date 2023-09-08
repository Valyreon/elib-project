using Valyreon.Elib.Domain;
using Valyreon.Elib.Mvvm.Messaging;

namespace Valyreon.Elib.Wpf.Messages
{
    public class OpenBookDetailsFlyoutMessage : MessageBase
    {
        public OpenBookDetailsFlyoutMessage(Book book)
        {
            Book = book;
        }

        public Book Book { get; }
    }
}
