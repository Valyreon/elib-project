using Domain;
using GalaSoft.MvvmLight.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
