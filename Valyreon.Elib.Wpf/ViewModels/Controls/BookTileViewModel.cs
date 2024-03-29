using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Input;
using Valyreon.Elib.DataLayer.Interfaces;
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
        private readonly LinkedListNode<Book> node;
        private readonly Selector selector;
        private readonly IUnitOfWorkFactory uowFactory;

        public BookTileViewModel(LinkedListNode<Book> bookNode, Selector selector, ApplicationProperties applicationProperties, IUnitOfWorkFactory uowFactory)
        {
            node = bookNode;
            Book = bookNode.Value;
            this.selector = selector;
            ApplicationProperties = applicationProperties;
            this.uowFactory = uowFactory;
        }

        public ICommand AddToCollectionCommand => new RelayCommand(HandleAddToCollection);
        public ApplicationProperties ApplicationProperties { get; }
        public Book Book { get; }
        public ICommand ExportCommand => new RelayCommand(HandleExport);

        public ICommand FavoriteMarkCommand => new RelayCommand(HandleMarkFavorite);

        public ICommand GoToAuthor =>
                            new RelayCommand(() => MessengerInstance.Send(new AuthorSelectedMessage(Book.Authors.First())));

        public ICommand GoToSeries =>
            new RelayCommand(() => Messenger.Default.Send(new SeriesSelectedMessage(Book.Series)));

        public ICommand OpenFileLocationCommand => new RelayCommand(OpenFileLocation);
        public ICommand OpenInReaderCommand => new RelayCommand(HandleOpenInReader);
        public ICommand ReadMarkCommand => new RelayCommand(HandleMarkRead);
        public ICommand RemoveCommand => new RelayCommand(HandleRemove);
        public ICommand SelectCommand => new RelayCommand(HandleSelectBook);
        public ICommand TileCommand => new RelayCommand(HandleBookClick);

        private void HandleAddToCollection()
        {
            var viewModel = new SimpleTextInputDialogViewModel("Add To Collection", "Enter collection name.", async tag =>
            {
                using var uow = await uowFactory.CreateAsync();
                var books = IsOnlyThisBookSelected() ? new List<Book> { Book } : await selector.GetSelectedBooks(uow);
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

        private void HandleBookClick()
        {
            var ctrlDown = Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl);
            var shiftDown = Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift);

            if (ctrlDown || shiftDown)
            {
                selector.Select(Book, ctrlDown);
                MessengerInstance.Send(new BookSelectedMessage(Book, ctrlDown, shiftDown));
            }
            else
            {
                Messenger.Default.Send(new OpenFlyoutMessage(new BookDetailsViewModel(node, ApplicationProperties, uowFactory)));
            }
        }

        private async void HandleExport()
        {
            if (IsOnlyThisBookSelected())
            {
                HandleSingleFileExport();
                return;
            }

            using var uow = await uowFactory.CreateAsync();
            var dialogViewModel = new ExportOptionsDialogViewModel(await selector.GetSelectedBooks(uow), uowFactory);
            MessengerInstance.Send(new ShowDialogMessage(dialogViewModel));
        }

        private async void HandleMarkFavorite()
        {
            if (IsOnlyThisBookSelected())
            {
                Book.IsFavorite = !Book.IsFavorite;
                using var uow1 = await uowFactory.CreateAsync();
                await uow1.BookRepository.UpdateAsync(Book);
                uow1.Commit();
                return;
            }

            var markBooksAs = !Book.IsFavorite;
            using var uow = await uowFactory.CreateAsync();
            var selectedBooks = await selector.GetSelectedBooks(uow);
            var booksToUpdate = new List<Book>();
            booksToUpdate.AddRange(selectedBooks.Where(b => b.IsFavorite != markBooksAs));
            booksToUpdate.ForEach(b => b.IsFavorite = markBooksAs);

            await uow.BookRepository.UpdateAsync(booksToUpdate);
            uow.Commit();
        }

        private async void HandleMarkRead()
        {
            if (IsOnlyThisBookSelected())
            {
                Book.IsRead = !Book.IsRead;
                using var uow1 = await uowFactory.CreateAsync();
                await uow1.BookRepository.UpdateAsync(Book);
                uow1.Commit();
                return;
            }

            var markBooksAs = !Book.IsRead;
            using var uow = await uowFactory.CreateAsync();
            var selectedBooks = await selector.GetSelectedBooks(uow);
            var booksToUpdate = new List<Book>();
            booksToUpdate.AddRange(selectedBooks.Where(b => b.IsRead != markBooksAs));
            booksToUpdate.ForEach(b => b.IsRead = markBooksAs);

            await uow.BookRepository.UpdateAsync(booksToUpdate);
            uow.Commit();
        }

        private void HandleOpenInReader()
        {
            var psi = new ProcessStartInfo
            {
                FileName = $@"""{Book.Path}""",
                // UseShellExecute set to true in order to use ShellExecute instead of the default CreateProcess to open a file.
                UseShellExecute = true
            };

            Process.Start(psi);
        }

        private async void HandleRemove()
        {
            IEnumerable<Book> books;
            using (var uow = await uowFactory.CreateAsync())
            {
                books = IsOnlyThisBookSelected() ? new List<Book> { Book } : await selector.GetSelectedBooks(uow);
            }
            var prompt = books.Count() == 1
                ? "Are you sure you want to remove this book from the library?\nFile will not be deleted."
                : "Are you sure you want to remove these books from the library?\nFiles will not be deleted.";
            var confirmViewModel = new ConfirmationDialogViewModel("Confirm Delete", prompt, async () =>
            {
                if (Book.IsMarked)
                {
                    selector.Select(Book);
                }

                using var uow = await uowFactory.CreateAsync();
                await uow.BookRepository.DeleteAsync(books);
                uow.Commit();
                MessengerInstance.Send(new BooksRemovedMessage(books));
                MessengerInstance.Send(new ShowNotificationMessage($"{books.Count()} book(s) removed from the library.", NotificationType.Success));
            });

            MessengerInstance.Send(new ShowDialogMessage(confirmViewModel));
        }

        private void HandleSelectBook()
        {
            selector.Select(Book);
            MessengerInstance.Send(new BookSelectedMessage(
                Book,
                Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl),
                Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift)));
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

        private bool IsOnlyThisBookSelected()
        {
            return !Book.IsMarked || selector.SelectedIds.Count() == 1;
        }

        private void OpenFileLocation()
        {
            if (File.Exists(Book.Path))
            {
                Process.Start("explorer.exe", "/select, " + $@"""{Book.Path}""");
            }
        }
    }
}
