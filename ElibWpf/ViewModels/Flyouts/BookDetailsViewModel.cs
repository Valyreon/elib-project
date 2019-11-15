using Domain;
using ElibWpf.Messages;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

namespace ElibWpf.ViewModels.Flyouts
{
    public class BookDetailsViewModel : ViewModelBase
    {
        public Book Book { get; private set; }

        public BookDetailsViewModel(Book book)
        {
            this.Book = book;
        }

        public ICommand GoToAuthor { get => new RelayCommand<ICollection<Author>>((ICollection<Author> a) => { Messenger.Default.Send(new AuthorSelectedMessage(a)); Messenger.Default.Send(new CloseFlyoutMessage()); }); }

        public ICommand GoToSeries { get => new RelayCommand<BookSeries>((BookSeries a) => { Messenger.Default.Send(new SeriesSelectedMessage(a)); Messenger.Default.Send(new CloseFlyoutMessage()); }); }

    }
}
