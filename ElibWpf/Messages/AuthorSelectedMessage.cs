using System.Collections.Generic;
using GalaSoft.MvvmLight.Messaging;
using Models.Observables;

namespace ElibWpf.Messages
{
    public class AuthorSelectedMessage : MessageBase
    {
        public AuthorSelectedMessage(IEnumerable<ObservableAuthor> selectedAuthors)
        {
            this.Authors = selectedAuthors;
        }

        public IEnumerable<ObservableAuthor> Authors { get; }
    }
}