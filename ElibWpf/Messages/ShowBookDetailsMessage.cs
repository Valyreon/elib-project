using Domain;

using GalaSoft.MvvmLight.Messaging;

namespace ElibWpf.Messages
{
    public class ShowBookDetailsMessage : MessageBase
    {
        public ShowBookDetailsMessage(Book clickedBook, bool edit = false)
        {
            this.Book = clickedBook;
            this.ToEdit = edit;
        }

        public Book Book { get; }

        public bool ToEdit { get; set; }
    }
}