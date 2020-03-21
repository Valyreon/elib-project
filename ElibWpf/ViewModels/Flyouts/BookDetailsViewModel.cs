using Domain;
using ElibWpf.Messages;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using OnlineBookApi.OpenLibrary;
using System;
using System.Collections.Generic;
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

        public ICommand LoadOnlineApiCommand { get => new RelayCommand(this.LoadOnlineApiAsync); }

        private async void LoadOnlineApiAsync()
        {
            // fill here stuff from online api
        }

        public bool IsBookRead
        {
            get => Book.IsRead;
            set
            {
                Book.IsRead = value;
                this.RaisePropertyChanged("IsBookRead");
                App.Database.SaveChangesAsync();
            }
        }

        public bool IsBookFavorite
        {
            get => Book.IsFavorite;
            set
            {
                Book.IsFavorite = value;
                this.RaisePropertyChanged("IsBookFavorite");
                App.Database.SaveChangesAsync();
            }
        }

        string bookDescription;
        public string BookDescriptionText
        {
            get => bookDescription;
            set => Set(ref bookDescription, value);
        }
    }
}
