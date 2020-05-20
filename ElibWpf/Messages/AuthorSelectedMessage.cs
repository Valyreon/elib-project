using System.Collections.Generic;
using GalaSoft.MvvmLight.Messaging;
using Models.Observables;

namespace ElibWpf.Messages
{
    public class AuthorSelectedMessage : MessageBase
    {
        public AuthorSelectedMessage(int authorId)
        {
            this.AuthorId = authorId;
        }

        public int AuthorId { get; }
    }
}