using Valyreon.Elib.Domain;

namespace Valyreon.Elib.Wpf.Messages
{
    public class EditBookMessage
    {
        public EditBookMessage(Book clickedBook)
        {
            Book = clickedBook;
        }

        public Book Book { get; }
    }
}
