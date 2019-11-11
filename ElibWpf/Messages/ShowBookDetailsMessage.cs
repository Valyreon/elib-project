using Domain;
using GalaSoft.MvvmLight.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElibWpf.Messages
{
    public class ShowBookDetailsMessage: MessageBase
    {
        public Book Book { get; }

        public ShowBookDetailsMessage(Book clickedBook)
        {
            this.Book = clickedBook;
        }
    }
}
