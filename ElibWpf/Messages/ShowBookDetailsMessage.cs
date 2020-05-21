using Models.Observables;
using MVVMLibrary.Messaging;

namespace ElibWpf.Messages
{
    public class ShowBookDetailsMessage : MessageBase
    {
        public ShowBookDetailsMessage(ObservableBook clickedBook)
        {
            this.Book = clickedBook;
        }

        public ObservableBook Book { get; }
    }
}