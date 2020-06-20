using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using DataLayer;
using Domain;
using ElibWpf.Messages;
using Models;

using MVVMLibrary;
using MVVMLibrary.Messaging;

namespace ElibWpf.ViewModels.Flyouts
{
    public class BookDetailsViewModel : ViewModelBase
    {
        private string addCollectionFieldText = "";

        private string bookDescription;

        public BookDetailsViewModel(Book book)
        {
            this.Book = book;
        }

        public ICommand AddCollectionCommand => new RelayCommand<string>(this.AddCollection);

        public string AddCollectionFieldText
        {
            get => this.addCollectionFieldText;
            set => base.Set(() => this.AddCollectionFieldText, ref this.addCollectionFieldText, value);
        }

        public Book Book { get; }

        public string BookDescriptionText
        {
            get => this.bookDescription;
            set => this.Set(() => BookDescriptionText, ref this.bookDescription, value);
        }

        public ICommand EditButtonCommand => new RelayCommand(this.HandleEditButton);

        public ICommand GoToAuthor => new RelayCommand<ICollection<Author>>(a =>
        {
            Messenger.Default.Send(new AuthorSelectedMessage(a.First()));
            Messenger.Default.Send(new CloseFlyoutMessage());
        });

        public ICommand GoToCollectionCommand => new RelayCommand<UserCollection>(this.GoToCollection);

        public ICommand GoToSeries => new RelayCommand<BookSeries>(a =>
        {
            Messenger.Default.Send(new SeriesSelectedMessage(a));
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
                    using var uow = ApplicationSettings.CreateUnitOfWork();
                    uow.BookRepository.Update(this.Book);
                    uow.Commit();
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
                    using var uow = ApplicationSettings.CreateUnitOfWork();
                    uow.BookRepository.Update(this.Book);
                    uow.Commit();
                });
            }
        }

        public ICommand LoadOnlineApiCommand => new RelayCommand(this.LoadOnlineApiAsync);

        public ICommand RemoveCollectionCommand => new RelayCommand<string>(this.RemoveCollection);

        private void GoToCollection(UserCollection obj)
        {
            this.MessengerInstance.Send(new CollectionSelectedMessage(obj.Id));
            this.MessengerInstance.Send(new CloseFlyoutMessage());
        }

        private void RemoveCollection(string tag)
        {
            UserCollection collection = this.Book.Collections.FirstOrDefault(c => c.Tag == tag);

            if (collection == null)
                return;

            this.Book.Collections.Remove(collection);

            Task.Run(() =>
            {
                using var uow = ApplicationSettings.CreateUnitOfWork();
                uow.CollectionRepository.RemoveCollectionForBook(collection, this.Book.Id);
                if (uow.CollectionRepository.CountBooksInUserCollection(collection.Id) <= 1)
                {
                    uow.CollectionRepository.Remove(collection);
                    this.MessengerInstance.Send(new RefreshSidePaneCollectionsMessage());
                }
                uow.Commit();
            });
        }

        private void AddCollection(string tag)
        {
            if (!string.IsNullOrWhiteSpace(tag))
            {
                tag = tag.Trim();
                this.AddCollectionFieldText = "";
                if (this.Book.Collections.Any(c => c.Tag == tag)) // check if book is already in that collection
                {
                    this.MessengerInstance.Send(new ShowDialogMessage("",
                        $"This book is already in the '{tag}' collection"));
                }
                else // if not
                {
                    UserCollection newCollection = new UserCollection {Tag = tag};
                    this.Book.Collections.Add(newCollection);
                    Task.Run(() =>
                    {
                        using var uow = ApplicationSettings.CreateUnitOfWork();
                        UserCollection existingCollection = uow.CollectionRepository.GetByTag(tag);
                        if (existingCollection == null)
                        {
                            uow.CollectionRepository.AddCollectionForBook(newCollection, this.Book.Id);
                            this.MessengerInstance.Send(new RefreshSidePaneCollectionsMessage());
                        }
                        else
                        {
                            newCollection.Id = existingCollection.Id;
                            uow.CollectionRepository.AddCollectionForBook(existingCollection, this.Book.Id);
                        }

                        uow.Commit();
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