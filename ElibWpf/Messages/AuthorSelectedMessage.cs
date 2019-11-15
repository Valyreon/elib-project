using Domain;

using GalaSoft.MvvmLight.Messaging;
using System.Collections.Generic;

namespace ElibWpf.Messages
{
    public class AuthorSelectedMessage : MessageBase
    {
        public AuthorSelectedMessage(IEnumerable<Author> selectedAuthors)
        {
            this.Authors = selectedAuthors;
        }

        public IEnumerable<Author> Authors { get; }
    }
}