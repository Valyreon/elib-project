using Valyreon.Elib.Domain;
using Valyreon.Elib.Mvvm.Messaging;

namespace Valyreon.Elib.Wpf.Messages
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
