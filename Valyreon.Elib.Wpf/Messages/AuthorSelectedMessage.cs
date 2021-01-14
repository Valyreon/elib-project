using Valyreon.Elib.Mvvm.Messaging;
using Valyreon.Elib.Domain;

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
