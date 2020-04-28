using Domain;

using GalaSoft.MvvmLight.Messaging;
using Models.Observables;
using System.Collections.Generic;

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