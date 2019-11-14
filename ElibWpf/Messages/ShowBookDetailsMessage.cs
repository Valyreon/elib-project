using Domain;

using GalaSoft.MvvmLight.Messaging;

namespace ElibWpf.Messages
{
    public class ShowBookDetailsMessage : MessageBase
    {
        public ShowBookDetailsMessage(Book clickedBook)
        {
            this.Book = clickedBook;
        }

        public Book Book { get; }
    }
}