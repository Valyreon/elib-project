using Domain;

using GalaSoft.MvvmLight;

namespace ElibWpf.ViewModels.Flyouts
{
    public class BookDetailsViewModel : ViewModelBase
    {
        private Book book;

        public BookDetailsViewModel(Book book)
        {
            this.book = book;
        }
    }
}