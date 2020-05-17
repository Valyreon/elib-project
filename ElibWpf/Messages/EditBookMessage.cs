using Models.Observables;

namespace ElibWpf.Messages
{
    public class EditBookMessage
    {
        public EditBookMessage(ObservableBook clickedBook)
        {
            this.Book = clickedBook;
        }

        public ObservableBook Book { get; }
    }
}