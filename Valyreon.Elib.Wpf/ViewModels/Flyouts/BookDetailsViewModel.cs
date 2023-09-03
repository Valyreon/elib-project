using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Valyreon.Elib.Domain;
using Valyreon.Elib.EBookTools;
using Valyreon.Elib.Mvvm;
using Valyreon.Elib.Mvvm.Messaging;
using Valyreon.Elib.Wpf.Messages;
using Valyreon.Elib.Wpf.Models;
using Valyreon.Elib.Wpf.ViewModels.Dialogs;

namespace Valyreon.Elib.Wpf.ViewModels.Flyouts
{
    public class BookDetailsViewModel : ViewModelBase
    {
        private ObservableCollection<ObservableEntity> collectionSuggestions;
        private IEnumerable<UserCollection> allUserCollections;

        public BookDetailsViewModel(Book book)
        {
            Book = book;
        }

        public ObservableCollection<ObservableEntity> CollectionSuggestions
        {
            get => collectionSuggestions;
            set => Set(() => CollectionSuggestions, ref collectionSuggestions, value);
        }

        public ICommand AddCollectionCommand => new RelayCommand<string>(AddCollection);

        public ICommand RefreshSuggestedCollectionsCommand => new RelayCommand<string>(HandleRefreshSuggestedCollections);

        private async void HandleRefreshSuggestedCollections(string token)
        {
            if (allUserCollections == null)
            {
                using var uow = await App.UnitOfWorkFactory.CreateAsync();
                allUserCollections = await uow.CollectionRepository.GetAllAsync();
            }

            var suggestions = allUserCollections.Where(c => !Book.Collections.Contains(c) && c.Tag.ToLowerInvariant().Contains(token))
                .Take(4);
            CollectionSuggestions = new ObservableCollection<ObservableEntity>(suggestions.Cast<ObservableEntity>());
        }

        public Book Book { get; }

        public ICommand EditButtonCommand => new RelayCommand(HandleEditButton);

        public ICommand GoToAuthor => new RelayCommand<ICollection<Author>>(a =>
        {
            Messenger.Default.Send(new AuthorSelectedMessage(a.First()));
            Messenger.Default.Send(new CloseFlyoutMessage());
        });

        public ICommand GoToCollectionCommand => new RelayCommand<UserCollection>(GoToCollection);

        public ICommand GoToSeries => new RelayCommand<BookSeries>(a =>
        {
            Messenger.Default.Send(new SeriesSelectedMessage(a));
            Messenger.Default.Send(new CloseFlyoutMessage());
        });

        public bool IsBookFavorite
        {
            get => Book.IsFavorite;
            set
            {
                Book.IsFavorite = value;
                RaisePropertyChanged(() => IsBookFavorite);

                Task.Run(async () =>
                {
                    using var uow = await App.UnitOfWorkFactory.CreateAsync();
                    await uow.BookRepository.UpdateAsync(Book);
                    uow.Commit();
                });
            }
        }

        public bool IsBookRead
        {
            get => Book.IsRead;
            set
            {
                Book.IsRead = value;
                RaisePropertyChanged(() => IsBookRead);

                Task.Run(async () =>
                {
                    using var uow = await App.UnitOfWorkFactory.CreateAsync();
                    await uow.BookRepository.UpdateAsync(Book);
                    uow.Commit();
                });
            }
        }

        public ICommand RemoveCollectionCommand => new RelayCommand<UserCollection>(RemoveCollection);

        private void GoToCollection(UserCollection obj)
        {
            MessengerInstance.Send(new CollectionSelectedMessage(obj.Id));
            MessengerInstance.Send(new CloseFlyoutMessage());
        }

        private void RemoveCollection(UserCollection collection)
        {
            Book.Collections.Remove(collection);

            Task.Run(async () =>
            {
                using var uow = await App.UnitOfWorkFactory.CreateAsync();
                await uow.CollectionRepository.RemoveCollectionForBookAsync(collection, Book.Id);
                if (await uow.CollectionRepository.CountBooksInUserCollectionAsync(collection.Id) == 0)
                {
                    await uow.CollectionRepository.DeleteAsync(collection);
                    MessengerInstance.Send(new RefreshSidePaneCollectionsMessage());
                }
                uow.Commit();
            });
        }

        private void AddCollection(string tag)
        {
            if (string.IsNullOrWhiteSpace(tag))
            {
                return;
            }

            tag = tag.Trim();
            if (Book.Collections.Any(c => c.Tag == tag)) // check if book is already in that collection
            {
                return;
            }

            var newCollection = new UserCollection { Tag = tag };
            Book.Collections.Add(newCollection);
            Task.Run(async () =>
            {
                using var uow = await App.UnitOfWorkFactory.CreateAsync();
                var existingCollection = await uow.CollectionRepository.GetByTagAsync(tag);
                if (existingCollection == null)
                {
                    await uow.CollectionRepository.AddCollectionForBookAsync(newCollection, Book.Id);
                    MessengerInstance.Send(new RefreshSidePaneCollectionsMessage());
                }
                else
                {
                    newCollection.Id = existingCollection.Id;
                    await uow.CollectionRepository.AddCollectionForBookAsync(existingCollection, Book.Id);
                }

                uow.Commit();
            });
        }

        private void HandleEditButton()
        {
            MessengerInstance.Send(new EditBookMessage(Book));
        }

        public ICommand ExportButtonCommand => new RelayCommand(HandleExport);

        private void HandleExport()
        {
            var dlg = new Microsoft.Win32.SaveFileDialog
            {
                FileName = Exporter.GenerateName(Book), // Default file name
                DefaultExt = Book.Format, // Default file extension
                CheckPathExists = true,
                Title = "Export book file",
                OverwritePrompt = true,
                Filter = $"Book file (*{Book.Format})|*{Book.Format}"
            };

            var result = dlg.ShowDialog();

            try
            {
                if (result == true)
                {
                    var filePath = dlg.FileName;
                    File.Copy(Book.Path, dlg.FileName);
                }
            }
            catch (Exception)
            {
                MessengerInstance.Send(new ShowNotificationMessage("Something went wrong while exporting the file.", NotificationType.Error));
            }
        }

        public ICommand ShowFileInfoCommand => new RelayCommand(HandleShowFileInfo);

        private void HandleShowFileInfo()
        {
            var builder = new StringBuilder("");

            builder.Append("File path: ").AppendLine(Book.Path);

            var strCheck = (string str) => string.IsNullOrWhiteSpace(str) ? "N/A" : str;

            if (File.Exists(Book.Path))
            {
                var x = EbookParserFactory.Create(Book.Path).Parse();
                builder.Append("ISBN: ").AppendLine(strCheck(x.Isbn));
                builder.Append("Publisher: ").AppendLine(strCheck(x.Publisher));
            }

            var viewModel = new TextMessageDialogViewModel("File Information", builder.ToString());
            MessengerInstance.Send(new ShowDialogMessage(viewModel));
        }
    }
}
