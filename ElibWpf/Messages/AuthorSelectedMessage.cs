using Domain;
using GalaSoft.MvvmLight.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElibWpf.Messages
{
    public class AuthorSelectedMessage: MessageBase
    {
        public Author Author { get; }

        public AuthorSelectedMessage(Author selectedAuthor)
        {
            this.Author = selectedAuthor;
        }
    }
}
