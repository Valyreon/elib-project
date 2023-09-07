using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Input;
using Valyreon.Elib.Domain;
using Valyreon.Elib.Mvvm;
using Valyreon.Elib.Mvvm.Messaging;
using Valyreon.Elib.Wpf.Messages;
using Valyreon.Elib.Wpf.Models;
using Valyreon.Elib.Wpf.ViewModels.Dialogs;
using Valyreon.Elib.Wpf.ViewModels.Flyouts;

namespace Valyreon.Elib.Wpf.ViewModels.Controls
{
    public class BookTileViewModel : ViewModelBase
    {
        public Book Book { get; }

        public BookTileViewModel(Book book)
        {
            Book = book;
        }

        public ICommand GoToAuthor =>
            new RelayCommand(() => MessengerInstance.Send(new AuthorSelectedMessage(Book.Authors.First())));

        public ICommand GoToSeries =>
            new RelayCommand(() => Messenger.Default.Send(new SeriesSelectedMessage(Book.Series)));

        public ICommand SelectCommand => new RelayCommand(HandleSelectBook);

        public ICommand OpenFileLocationCommand => new RelayCommand(OpenFileLocation);

        public ICommand TileCommand => new RelayCommand(HandleBookClick);

        public ICommand ExportCommand => new RelayCommand(HandleExport);

        public ICommand ReadMarkCommand => new RelayCommand(HandleMarkRead);

        public ICommand OpenInReaderCommand => new RelayCommand(HandleOpenInReader);

        private bool IsOnlyThisBookSelected()
        {
            return !Book.IsMarked || Selector.Instance.SelectedIds.Count() == 1;
        }

        private async void HandleMarkRead()
        {
            if (IsOnlyThisBookSelected())
            {
                Book.IsRead = !Book.IsRead;
                using var uow1 = await App.UnitOfWorkFactory.CreateAsync();
                await uow1.BookRepository.UpdateAsync(Book);
                uow1.Commit();
                return;
            }

            var markBooksAs = !Book.IsRead;
            using var uow = await App.UnitOfWorkFactory.CreateAsync();
            var selectedBooks = await Selector.Instance.GetSelectedBooks(uow);
            var booksToUpdate = new List<Book>();
            booksToUpdate.AddRange(selectedBooks.Where(b => b.IsRead != markBooksAs));
            booksToUpdate.ForEach(b => b.IsRead = markBooksAs);

            await uow.BookRepository.UpdateAsync(booksToUpdate);
            uow.Commit();
            return;
        }

        public ICommand FavoriteMarkCommand => new RelayCommand(HandleMarkFavorite);

        private async void HandleMarkFavorite()
        {
            if (IsOnlyThisBookSelected())
            {
                Book.IsFavorite = !Book.IsFavorite;
                using var uow1 = await App.UnitOfWorkFactory.CreateAsync();
                await uow1.BookRepository.UpdateAsync(Book);
                uow1.Commit();
                return;
            }

            var markBooksAs = !Book.IsFavorite;
            using var uow = await App.UnitOfWorkFactory.CreateAsync();
            var selectedBooks = await Selector.Instance.GetSelectedBooks(uow);
            var booksToUpdate = new List<Book>();
            booksToUpdate.AddRange(selectedBooks.Where(b => b.IsFavorite != markBooksAs));
            booksToUpdate.ForEach(b => b.IsFavorite = markBooksAs);

            await uow.BookRepository.UpdateAsync(booksToUpdate);
            uow.Commit();
            return;
        }

        private void HandleSelectBook()
        {
            Selector.Instance.Select(Book);
            MessengerInstance.Send(new BookSelectedMessage(
                Book,
                Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl),
                Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift)));
        }

        private void HandleBookClick()
        {
            var ctrlDown = Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl);
            var shiftDown = Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift);

            if (ctrlDown || shiftDown)
            {
                Selector.Instance.Select(Book, false);
                MessengerInstance.Send(new BookSelectedMessage(Book, ctrlDown, shiftDown));
            }
            else
            {
                Messenger.Default.Send(new OpenFlyoutMessage(new BookDetailsViewModel(Book)));
            }
        }

        private void OpenFileLocation()
        {
            if (File.Exists(Book.Path))
            {
                Process.Start("explorer.exe", "/select, " + $@"""{Book.Path}""");
            }
        }

        private void HandleOpenInReader()
        {
            var appSettings = ApplicationData.GetProperties();

            if(!string.IsNullOrWhiteSpace(appSettings.ExternalReaderPath) && File.Exists(appSettings.ExternalReaderPath))
            {
                Process.Start(appSettings.ExternalReaderPath, $@"""{Book.Path}""");
            }
        }


        private async void HandleExport()
        {
            if (IsOnlyThisBookSelected())
            {
                HandleSingleFileExport();
                return;
            }

            using var uow = await App.UnitOfWorkFactory.CreateAsync();
            var dialogViewModel = new ExportOptionsDialogViewModel(await Selector.Instance.GetSelectedBooks(uow));
            MessengerInstance.Send(new ShowDialogMessage(dialogViewModel));
        }

        private void HandleSingleFileExport()
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

        public ICommand RemoveCommand => new RelayCommand(HandleRemove);

        public ICommand AddToCollectionCommand => new RelayCommand(HandleAddToCollection);

        private void HandleAddToCollection()
        {
            var viewModel = new SimpleTextInputDialogViewModel("Add To Collection", "Enter collection name.", async tag =>
            {
                using var uow = await App.UnitOfWorkFactory.CreateAsync();
                var books = IsOnlyThisBookSelected() ? new List<Book> { Book } : await Selector.Instance.GetSelectedBooks(uow);
                var collection = await uow.CollectionRepository.GetByTagAsync(tag);
                if (collection == null)
                {
                    var newCollection = new UserCollection
                    {
                        Tag = tag
                    };
                    await uow.CollectionRepository.CreateAsync(newCollection);
                    uow.Commit();
                    collection = newCollection;
                    MessengerInstance.Send(new RefreshSidePaneCollectionsMessage());
                }

                foreach (var book in books)
                {
                    book.Collections.Add(collection);
                    await uow.CollectionRepository.AddCollectionForBookAsync(collection, book.Id);
                }

                uow.Commit();
            });

            MessengerInstance.Send(new ShowDialogMessage(viewModel));
        }

        private async void HandleRemove()
        {
            IEnumerable<Book> books;
            using (var uow = await App.UnitOfWorkFactory.CreateAsync())
            {
                books = IsOnlyThisBookSelected() ? new List<Book> { Book } : await Selector.Instance.GetSelectedBooks(uow);
            }
            var prompt = books.Count() == 1
                ? "Are you sure you want to remove this book from the library?\nFile will not be deleted."
                : "Are you sure you want to remove these books from the library?\nFiles will not be deleted.";
            var confirmViewModel = new ConfirmationDialogViewModel("Confirm Delete", prompt, async () =>
            {
                if (Book.IsMarked)
                {
                    Selector.Instance.Select(Book);
                }

                using var uow = await App.UnitOfWorkFactory.CreateAsync();
                await uow.BookRepository.DeleteAsync(books);
                uow.Commit();
                MessengerInstance.Send(new BooksRemovedMessage(books));
                MessengerInstance.Send(new ShowNotificationMessage($"{books.Count()} book(s) removed from the library.", NotificationType.Success));
            });

            MessengerInstance.Send(new ShowDialogMessage(confirmViewModel));
        }
    }
}
