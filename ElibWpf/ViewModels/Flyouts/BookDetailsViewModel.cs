using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using DataLayer;
using Domain;
using ElibWpf.Messages;
using Models;
using Models.Observables;
using MVVMLibrary;
using MVVMLibrary.Messaging;

namespace ElibWpf.ViewModels.Flyouts
{
    public class BookDetailsViewModel : ViewModelBase
    {
        private string addCollectionFieldText = "";

        private string bookDescription;

        public BookDetailsViewModel(ObservableBook book)
        {
            this.Book = book;
        }

        public ICommand AddCollectionCommand => new RelayCommand<string>(this.AddCollection);

        public string AddCollectionFieldText
        {
            get => this.addCollectionFieldText;
            set => base.Set(() => this.AddCollectionFieldText, ref this.addCollectionFieldText, value);
        }

        public ObservableBook Book { get; }

        public string BookDescriptionText
        {
            get => this.bookDescription;
            set => this.Set(() => BookDescriptionText, ref this.bookDescription, value);
        }

        public ICommand EditButtonCommand => new RelayCommand(this.HandleEditButton);

        public ICommand GoToAuthor => new RelayCommand<ICollection<ObservableAuthor>>(a =>
        {
            Messenger.Default.Send(new AuthorSelectedMessage(a.First().Id));
            Messenger.Default.Send(new CloseFlyoutMessage());
        });

        public ICommand GoToCollectionCommand => new RelayCommand<ObservableUserCollection>(this.GoToCollection);

        public ICommand GoToSeries => new RelayCommand<ObservableSeries>(a =>
        {
            Messenger.Default.Send(new SeriesSelectedMessage(a.Id));
            Messenger.Default.Send(new CloseFlyoutMessage());
        });

        public bool IsBookFavorite
        {
            get => this.Book.IsFavorite;
            set
            {
                this.Book.IsFavorite = value;
                this.RaisePropertyChanged(() => this.IsBookFavorite);

                Task.Run(() =>
                {
                    using ElibContext database = ApplicationSettings.CreateContext();
                    database.Entry(this.Book.Book).State = EntityState.Modified;
                    database.SaveChanges();
                });
            }
        }

        public bool IsBookRead
        {
            get => this.Book.IsRead;
            set
            {
                this.Book.IsRead = value;
                this.RaisePropertyChanged(() => this.IsBookRead);

                Task.Run(() =>
                {
                    using ElibContext database = ApplicationSettings.CreateContext();
                    database.Entry(this.Book.Book).State = EntityState.Modified;
                    database.SaveChanges();
                });
            }
        }

        public ICommand LoadOnlineApiCommand => new RelayCommand(this.LoadOnlineApiAsync);

        public ICommand RemoveCollectionCommand => new RelayCommand<string>(this.RemoveCollection);

        private void GoToCollection(ObservableUserCollection obj)
        {
            this.MessengerInstance.Send(new CollectionSelectedMessage(obj.Id));
            this.MessengerInstance.Send(new CloseFlyoutMessage());
        }

        private void RemoveCollection(string tag)
        {
            ObservableUserCollection collection = this.Book.Collections.FirstOrDefault(c => c.Tag == tag);

            if (collection == null)
                return;

            this.Book.Collections.Remove(collection);

            Task.Run(() =>
            {
                using ElibContext database = ApplicationSettings.CreateContext();
                database.Books.Attach(this.Book.Book);
                if (database.Books.Count(b => b.UserCollections.Any(c => c.Tag == tag)) <= 1)
                {
                    database.Entry(collection.Collection).State = EntityState.Deleted;
                    this.MessengerInstance.Send(new RefreshSidePaneCollectionsMessage());
                }

                database.SaveChanges();
            });
        }

        private void AddCollection(string tag)
        {
            if (!string.IsNullOrWhiteSpace(tag))
            {
                tag = tag.Trim();
                bool isNew = false;
                this.AddCollectionFieldText = "";
                if (this.Book.Collections.Any(c => c.Tag == tag)) // check if book is already in that collection
                {
                    this.MessengerInstance.Send(new ShowDialogMessage("",
                        $"This book is already in the '{tag}' collection"));
                }
                else // if not
                {
                    ObservableUserCollection newCollection = new ObservableUserCollection(new UserCollection {Tag = tag});
                    this.Book.Collections.Add(newCollection);
                    Task.Run(() =>
                    {
                        using ElibContext database = ApplicationSettings.CreateContext();
                        database.Books.Attach(this.Book.Book);
                        UserCollection existingCollection = database.UserCollections.FirstOrDefault(c => c.Tag == tag);
                        if (existingCollection == null)
                        {
                            isNew = true;
                            this.Book.Book.UserCollections.Add(newCollection.Collection);
                        }
                        else
                        {
                            this.Book.Collections.Remove(newCollection);
                            this.Book.Collections.Add(new ObservableUserCollection(existingCollection));
                        }

                        database.SaveChanges();
                        if (isNew)
                        {
                            this.MessengerInstance.Send(new RefreshSidePaneCollectionsMessage());
                        }
                    });
                }
            }
        }

        private void LoadOnlineApiAsync()
        {
            // fill here stuff from online api
        }

        private void HandleEditButton()
        {
            this.MessengerInstance.Send(new EditBookMessage(this.Book));
        }
    }
}