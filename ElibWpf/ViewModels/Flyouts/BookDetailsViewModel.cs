using DataLayer;
using Domain;
using ElibWpf.Messages;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ElibWpf.ViewModels.Flyouts
{
    public class BookDetailsViewModel : ViewModelBase
    {
        public Book Book { get; private set; }

        public BookDetailsViewModel(Book book)
        {
            this.Book = book;
            UserCollections = new ObservableCollection<UserCollection>(Book.UserCollections);
        }

        public ObservableCollection<UserCollection> UserCollections { get; }

        public ICommand GoToAuthor { get => new RelayCommand<ICollection<Author>>((ICollection<Author> a) => { Messenger.Default.Send(new AuthorSelectedMessage(a)); Messenger.Default.Send(new CloseFlyoutMessage()); }); }

        public ICommand GoToSeries { get => new RelayCommand<BookSeries>((BookSeries a) => { Messenger.Default.Send(new SeriesSelectedMessage(a)); Messenger.Default.Send(new CloseFlyoutMessage()); }); }

        public ICommand LoadOnlineApiCommand { get => new RelayCommand(this.LoadOnlineApiAsync); }

        public ICommand AddCollectionCommand { get => new RelayCommand<string>(this.AddCollection); }

        public ICommand RemoveCollectionCommand { get => new RelayCommand<string>(this.RemoveCollection); }

        public ICommand GoToCollectionCommand { get => new RelayCommand<UserCollection>(this.GoToCollection); }

        private void GoToCollection(UserCollection obj)
        {
            MessengerInstance.Send(new CollectionSelectedMessage(obj));
            MessengerInstance.Send(new CloseFlyoutMessage());
        }

        private void RemoveCollection(string tag)
        {
            var collection = Book.UserCollections.Where(c => c.Tag == tag).FirstOrDefault();
            Book.UserCollections.Remove(collection);
            UserCollections.Remove(collection);

            Task.Run(() =>
            {
                using ElibContext database = ApplicationSettings.CreateContext();
                database.Books.Attach(Book);
                Book.UserCollections.Remove(collection);
                if (database.Books.Where(b => b.UserCollections.Where(c => c.Tag == tag).Any()).Count() == 1)
                {
                    database.Entry(collection).State = EntityState.Deleted;
                    MessengerInstance.Send(new RefreshSidePaneCollectionsMessage());
                }
                database.SaveChanges();
            });

        }

        private string addCollectionFieldText = "";

        public string AddCollectionFieldText
        {
            get => addCollectionFieldText;
            set => base.Set(() => AddCollectionFieldText, ref addCollectionFieldText, value);
        }

        private void AddCollection(string tag)
        {
            if (!string.IsNullOrWhiteSpace(tag))
            {
                tag = tag.Trim();
                bool isNew = false;
                AddCollectionFieldText = "";
                if (Book.UserCollections.Where(c => c.Tag == tag).Any()) // check if book is already in that collection
                {
                    MessengerInstance.Send(new ShowDialogMessage("", $"This book is already in the '{tag}' collection"));
                }
                else // if not
                {
                    UserCollection newCollection = new UserCollection { Tag = tag };
                    UserCollections.Add(newCollection);
                    Task.Run(() =>
                    {
                        using ElibContext database = ApplicationSettings.CreateContext();
                        database.Books.Attach(Book);
                        var existingCollection = database.UserCollections.Where(c => c.Tag == tag).FirstOrDefault();
                        if (existingCollection == null)
                        {
                            isNew = true;
                            Book.UserCollections.Add(newCollection);
                        }
                        else
                        {
                            Book.UserCollections.Add(existingCollection);
                        }

                        database.SaveChanges();
                        if (isNew)
                            MessengerInstance.Send(new RefreshSidePaneCollectionsMessage());
                    });
                }
            }
        }

        private void LoadOnlineApiAsync()
        {
            // fill here stuff from online api
        }

        public bool IsBookRead
        {
            get => Book.IsRead;
            set
            {
                Book.IsRead = value;
                this.RaisePropertyChanged(() => IsBookRead);

                Task.Run(() =>
                {
                    using ElibContext database = ApplicationSettings.CreateContext();
                    database.Entry(Book).State = EntityState.Modified;
                    database.SaveChanges();
                });
            }
        }

        public bool IsBookFavorite
        {
            get => Book.IsFavorite;
            set
            {
                Book.IsFavorite = value;
                this.RaisePropertyChanged(() => IsBookFavorite);

                Task.Run(() =>
                {
                    using ElibContext database = ApplicationSettings.CreateContext();
                    database.Entry(Book).State = EntityState.Modified;
                    database.SaveChanges();
                });
            }
        }

        private string bookDescription;

        public string BookDescriptionText
        {
            get => bookDescription;
            set => Set(ref bookDescription, value);
        }

        public ICommand EditButtonCommand { get => new RelayCommand(this.HandleEditButton); }

        private void HandleEditButton()
        {
            MessengerInstance.Send(new EditBookMessage(Book));
        }
    }
}