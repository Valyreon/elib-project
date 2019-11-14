using Domain;

using GalaSoft.MvvmLight.Messaging;

namespace ElibWpf.Messages
{
    public class AuthorSelectedMessage : MessageBase
    {
        public AuthorSelectedMessage(Author selectedAuthor)
        {
            this.Author = selectedAuthor;
        }

        public Author Author { get; }
    }
}