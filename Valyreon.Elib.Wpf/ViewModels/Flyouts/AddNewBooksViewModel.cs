using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using MahApps.Metro.Controls.Dialogs;
using Valyreon.Elib.Domain;
using Valyreon.Elib.EBookTools;
using Valyreon.Elib.Mvvm;
using Valyreon.Elib.Wpf.Extensions;
using Valyreon.Elib.Wpf.Messages;
using Valyreon.Elib.Wpf.Models;
using Valyreon.Elib.Wpf.ValidationAttributes;
using Valyreon.Elib.Wpf.ViewModels.Controls;
using Valyreon.Elib.Wpf.ViewModels.Dialogs;
using Valyreon.Elib.Wpf.Views.Dialogs;
using Application = System.Windows.Application;

namespace Valyreon.Elib.Wpf.ViewModels.Flyouts
{
    public class AddNewBooksViewModel : ViewModelWithValidation
    {
        private readonly IList<string> books;

        private int counter;

        private Book currentBook;

        private bool isCurrentDuplicate = true;

        private bool isSaving;

        private string proceedButtonText;

        private string warning;

        private string path;

        private bool isLoading;
        private string titleText;
        private EditBookFormViewModel editBookForm;

        public AddNewBooksViewModel(IEnumerable<string> newBooks)
        {
            books = newBooks.ToList();
        }

        public ICommand CancelButtonCommand => new RelayCommand(HandleCancel);

        public bool IsCurrentBookDuplicate
        {
            get => isSaving || isCurrentDuplicate;
            set => Set(() => IsCurrentBookDuplicate, ref isCurrentDuplicate, value);
        }

        public bool IsLoading
        {
            get => isLoading;
            set => Set(() => IsLoading, ref isLoading, value);
        }

        public bool IsSaving
        {
            get => isSaving;
            set
            {
                Set(() => IsSaving, ref isSaving, value);
                RaisePropertyChanged(() => IsCurrentBookDuplicate);
            }
        }

        public ICommand LoadedCommand => new RelayCommand(HandleLoaded);

        public ICommand NextButtonCommand => new RelayCommand(HandleSaveAndNext);

        public string ProceedButtonText
        {
            get => proceedButtonText;
            set => Set(() => ProceedButtonText, ref proceedButtonText, value);
        }

        public ICommand RevertButtonCommand => new RelayCommand(HandleRevert);

        public ICommand SkipButtonCommand => new RelayCommand(NextBook);

        public string TitleText
        {
            get => titleText;
            set => Set(() => TitleText, ref titleText, value);
        }

        public string WarningText
        {
            get => warning;
            set => Set(() => WarningText, ref warning, value);
        }

        public string PathText
        {
            get => path;
            set => Set(() => PathText, ref path, value);
        }

        public EditBookFormViewModel EditBookForm
        {
            get => editBookForm;
            set => Set(() => EditBookForm, ref editBookForm, value);
        }

        private Book CurrentBook
        {
            get => currentBook;

            set
            {
                currentBook = value;
                Set(() => CurrentBook, ref currentBook, value);
                if (currentBook != null)
                {
                    EditBookForm = new EditBookFormViewModel(currentBook);

                    PathText = currentBook.Path;
                }
            }
        }

        private async Task<Book> ParseBook(string path)
        {
            IsLoading = true;
            Book result = null;
            try
            {
                await Task.Run(async () =>
                {
                    var pBook = EbookParserFactory.Create(path).Parse();
                    using var uow = await App.UnitOfWorkFactory.CreateAsync();
                    var book = await pBook.ToBookAsync(uow);
                    result = book;
                });
            }
            catch (Exception)
            {
                result = new Book
                {
                    Collections = new ObservableCollection<UserCollection>(),
                    Format = Path.GetExtension(path),
                    Signature = Signer.ComputeHash(path),
                    Authors = new ObservableCollection<Author>(),
                    Path = path
                };
            }
            finally
            {
                IsLoading = false;
            }

            return result;
        }

        private async void HandleLoaded()
        {
            TitleText = $"Book 1 of {books.Count}";
            PathText = books[0];
            CurrentBook = await ParseBook(books[0]);
            
            ProceedButtonText = books.Count == 1 ? "SAVE & FINISH" : "SAVE & NEXT";
            CheckDuplicate(CurrentBook);
        }

        private void HandleRevert()
        {
            ClearErrors();
            EditBookForm = new EditBookFormViewModel(currentBook);
        }

        private void HandleSaveAndNext()
        {
            IsSaving = true;

            if (EditBookForm.CreateBook())
            {
                NextBook();
            };
            
            IsSaving = false;
        }

        private void HandleCancel()
        {
            MessengerInstance.Send(new CloseFlyoutMessage());
        }

        private async void NextBook()
        {
            if (counter >= books.Count - 1)
            {
                MessengerInstance.Send(new CloseFlyoutMessage());
                MessengerInstance.Send(new RefreshCurrentViewMessage());
            }
            else
            {
                ClearErrors();
                TitleText = $"Book {counter + 2} of {books.Count}";
                if (counter == books.Count - 2)
                {
                    ProceedButtonText = "SAVE & FINISH";
                }

                var nextBook = books[++counter];
                PathText = nextBook;
                CurrentBook = await ParseBook(nextBook);

                CheckDuplicate(CurrentBook);
            }
        }

        private async void CheckDuplicate(Book book)
        {
            using var uow = await App.UnitOfWorkFactory.CreateAsync();
            if (await uow.BookRepository.SignatureExistsAsync(book.Signature))
            {
                WarningText = "This book is a duplicate of a book already in the database.";
                IsCurrentBookDuplicate = true;
            }
            else
            {
                WarningText = null;
                IsCurrentBookDuplicate = false;
            }
        }
    }
}
