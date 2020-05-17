using System.Collections.Generic;
using Domain;
using GalaSoft.MvvmLight.Messaging;

namespace ElibWpf.Messages
{
    public class OpenAddBooksFormMessage : MessageBase
    {
        public OpenAddBooksFormMessage(IList<Book> booksToAdd)
        {
            this.BooksToAdd = booksToAdd;
        }

        public IList<Book> BooksToAdd { get; }
    }
}