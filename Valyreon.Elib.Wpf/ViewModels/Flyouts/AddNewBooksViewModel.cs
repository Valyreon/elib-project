using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Valyreon.Elib.DataLayer.Interfaces;
using Valyreon.Elib.Domain;
using Valyreon.Elib.EBookTools;
using Valyreon.Elib.Mvvm;
using Valyreon.Elib.Wpf.Extensions;
using Valyreon.Elib.Wpf.Messages;
using Valyreon.Elib.Wpf.Models;
using Valyreon.Elib.Wpf.ViewModels.Controls;

namespace Valyreon.Elib.Wpf.ViewModels.Flyouts
{
    public class AddNewBooksViewModel : ViewModelWithValidation
    {
        private readonly IList<string> books;
        private readonly IUnitOfWorkFactory uowFactory;
        private int counter;

        private Book currentBook;

        private EditBookFormViewModel editBookForm;
        private bool isCurrentDuplicate = true;

        private bool isLoading;
        private bool isSaving;

        private string path;
        private string proceedButtonText;

        private string titleText;
        private string warning;

        public AddNewBooksViewModel(IEnumerable<string> newBooks, IUnitOfWorkFactory uowFactory)
        {
            books = newBooks.ToList();
            this.uowFactory = uowFactory;
        }

        public ICommand CancelButtonCommand => new RelayCommand(HandleCancel);

        public EditBookFormViewModel EditBookForm
        {
            get => editBookForm;
            set => Set(() => EditBookForm, ref editBookForm, value);
        }

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

        public string PathText
        {
            get => path;
            set => Set(() => PathText, ref path, value);
        }

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

        private Book CurrentBook
        {
            get => currentBook;

            set
            {
                currentBook = value;
                Set(() => CurrentBook, ref currentBook, value);
                if (currentBook != null)
                {
                    EditBookForm = new EditBookFormViewModel(currentBook, uowFactory);

                    PathText = currentBook.Path;
                }
            }
        }

        private async void CheckDuplicate(Book book)
        {
            using var uow = await uowFactory.CreateAsync();
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

        private void HandleCancel()
        {
            MessengerInstance.Send(new CloseFlyoutMessage());
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
            EditBookForm = new EditBookFormViewModel(currentBook, uowFactory);
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

        private async Task<Book> ParseBook(string path)
        {
            IsLoading = true;
            Book result = null;
            try
            {
                await Task.Run(async () =>
                {
                    var pBook = EbookParserFactory.Create(path).Parse();
                    var book = await pBook.ToBookAsync(uowFactory);
                    result = book;
                });
            }
            catch (Exception ex)
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
    }
}
