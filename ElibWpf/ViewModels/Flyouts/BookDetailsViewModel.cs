using Domain;
using ElibWpf.Messages;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ElibWpf.ViewModels.Flyouts
{
    public class BookDetailsViewModel : ViewModelBase
    {
        public Book Book { get; private set; }

        public BookDetailsViewModel(Book book, bool edit = false)
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

        private async void RemoveCollection(string tag)
        {
            var collection = Book.UserCollections.Where(c => c.Tag == tag).FirstOrDefault();
            Book.UserCollections.Remove(collection);

            await Task.Run(() =>
            {
                if (App.Database.Books.Where(b => b.UserCollections.Where(c => c.Tag == tag).Any()).Count() == 1)
                {
                    App.Database.UserCollections.Remove(collection);
                    App.Database.SaveChanges();
                    MessengerInstance.Send(new RefreshSidePaneCollectionsMessage());
                }
            });

            await App.Database.SaveChangesAsync();
            UserCollections.Remove(collection);
        }

        private string addCollectionFieldText = "";

        public string AddCollectionFieldText
        {
            get => addCollectionFieldText;
            set => base.Set(() => AddCollectionFieldText, ref addCollectionFieldText, value);
        }

        private async void AddCollection(string tag)
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
                    var existingCollection = await Task.Run(() => App.Database.UserCollections.Where(c => c.Tag == tag).FirstOrDefault());
                    if (existingCollection == null)
                    {
                        UserCollection newCollection = new UserCollection
                        {
                            Tag = tag
                        };
                        Book.UserCollections.Add(newCollection);
                        UserCollections.Add(newCollection);
                        isNew = true;
                    }
                    else
                    {
                        Book.UserCollections.Add(existingCollection);
                        UserCollections.Add(existingCollection);
                    }

                    await App.Database.SaveChangesAsync();
                    if (isNew)
                        MessengerInstance.Send(new RefreshSidePaneCollectionsMessage());
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

        private string bookDescription;

        public string BookDescriptionText
        {
            get => bookDescription;
            set => Set(ref bookDescription, value);
        }
    }
}