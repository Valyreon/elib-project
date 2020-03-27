using Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElibWpf.Messages
{
    public class EditBookMessage
    {
        public EditBookMessage(Book clickedBook)
        {
            this.Book = clickedBook;
        }

        public Book Book { get; }
    }
}
