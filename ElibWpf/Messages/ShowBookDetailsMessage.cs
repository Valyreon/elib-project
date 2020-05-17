using GalaSoft.MvvmLight.Messaging;
using Models.Observables;

namespace ElibWpf.Messages
{
    public class ShowBookDetailsMessage : MessageBase
    {
        public ShowBookDetailsMessage(ObservableBook clickedBook, bool edit = false)
        {
            this.Book = clickedBook;
            this.ToEdit = edit;
        }

        public ObservableBook Book { get; }

        public bool ToEdit { get; set; }
    }
}