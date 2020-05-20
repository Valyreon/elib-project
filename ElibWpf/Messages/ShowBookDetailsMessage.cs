using GalaSoft.MvvmLight.Messaging;
using Models.Observables;

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