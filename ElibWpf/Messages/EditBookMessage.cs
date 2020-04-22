using Domain;

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