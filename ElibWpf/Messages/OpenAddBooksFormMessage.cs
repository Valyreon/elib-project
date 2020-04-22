using Domain;
using GalaSoft.MvvmLight.Messaging;
using System.Collections.Generic;

namespace ElibWpf.Messages
{
    public class OpenAddBooksFormMessage : MessageBase
    {
        public IList<Book> BooksToAdd { get; }

        public OpenAddBooksFormMessage(IList<Book> booksToAdd)
        {
            BooksToAdd = booksToAdd;
        }
    }
}