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
using Valyreon.Elib.Wpf.ViewModels.Dialogs;
using Valyreon.Elib.Wpf.Views.Dialogs;
using Application = System.Windows.Application;

namespace Valyreon.Elib.Wpf.ViewModels.Flyouts
{
    public class AddNewBooksViewModel : ViewModelWithValidation
    {
        private readonly IList<string> books;

        private string addAuthorFieldText;

        private ObservableCollection<Author> authorCollection;

        private int counter;

        private Cover coverImage;

        private Book currentBook;

        private bool isCurrentDuplicate = true;

        private bool isFavorite;

        private bool isRead;

        private bool isSaving;

        private string proceedButtonText;

        private BookSeries series;

        private string seriesNumberFieldText;

        private string titleFieldText;

        private string titleText;

        private string warning;

        public AddNewBooksViewModel(IEnumerable<string> newBooks)
        {
            books = newBooks.ToList();
        }

        public string AddAuthorFieldText
        {
            get => addAuthorFieldText;
            set => Set(() => AddAuthorFieldText, ref addAuthorFieldText, value);
        }

        public ICommand AddExistingAuthorButtonCommand => new RelayCommand(HandleAddExistingAuthor);

        public ICommand AddNewAuthorButtonCommand => new RelayCommand(HandleAddNewAuthor);

        [NotEmpty(ErrorMessage = "Book has to have at least one author.")]
        public ObservableCollection<Author> AuthorsCollection
        {
            get => authorCollection;
            private set => Set(() => AuthorsCollection, ref authorCollection, value);
        }

        public ICommand CancelButtonCommand => new RelayCommand(HandleCancel);

        public ICommand ChangeCoverButtonCommand => new RelayCommand(HandleChangeCoverButton);

        public ICommand ChooseExistingSeriesCommand => new RelayCommand(HandleChooseExistingSeries);

        public ICommand ClearSeriesCommand => new RelayCommand(HandleClearSeries);

        public Cover Cover
        {
            get => coverImage;
            set => Set(() => Cover, ref coverImage, value);
        }

        public ICommand CreateNewSeriesCommand => new RelayCommand(HandleCreateNewSeries);

        public ICommand EditSeriesCommand => new RelayCommand(HandleEditSeries);

        public bool IsCurrentBookDuplicate
        {
            get => isSaving || isCurrentDuplicate;
            set => Set(() => IsCurrentBookDuplicate, ref isCurrentDuplicate, value);
        }

        public bool IsFavoriteCheck
        {
            get => isFavorite;
            set => Set(() => IsFavoriteCheck, ref isFavorite, value);
        }

        public bool IsReadCheck
        {
            get => isRead;
            set => Set(() => IsReadCheck, ref isRead, value);
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

        public bool IsSeriesSelected => Series != null;

        public ICommand LoadedCommand => new RelayCommand(HandleLoaded);

        public ICommand NextButtonCommand => new RelayCommand(HandleSaveAndNext);

        public string ProceedButtonText
        {
            get => proceedButtonText;
            set => Set(() => ProceedButtonText, ref proceedButtonText, value);
        }

        public ICommand RemoveAuthorCommand =>
            new RelayCommand<int>(x => AuthorsCollection.Remove(AuthorsCollection.FirstOrDefault(c => c.Id == x)));

        public ICommand RemoveCoverButtonCommand => new RelayCommand(() => Cover = null);

        public ICommand RevertButtonCommand => new RelayCommand(HandleRevert);

        public BookSeries Series
        {
            get => series;
            set
            {
                Set(() => Series, ref series, value);
                RaisePropertyChanged(() => IsSeriesSelected);
                RaisePropertyChanged(() => SeriesColor);
            }
        }

        public Brush SeriesColor => !IsSeriesSelected ? Brushes.Gray : (Brush)new BrushConverter().ConvertFrom("#bbb");

        public string SeriesNumberFieldText
        {
            get => seriesNumberFieldText;
            set => Set(() => SeriesNumberFieldText, ref seriesNumberFieldText, value);
        }

        public ICommand SkipButtonCommand => new RelayCommand(NextBook);

        [Required(ErrorMessage = "Book title can't be empty.")]
        public string TitleFieldText
        {
            get => titleFieldText;
            set => Set(() => TitleFieldText, ref titleFieldText, value);
        }

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
                    AuthorsCollection = new ObservableCollection<Author>(CurrentBook.Authors);

                    Series = CurrentBook.Series != null
                        ? new BookSeries() { Id = CurrentBook.Series.Id, Name = CurrentBook.Series.Name }
                        : null;

                    TitleFieldText = CurrentBook.Title;
                    SeriesNumberFieldText = CurrentBook.NumberInSeries.ToString();
                    IsFavoriteCheck = CurrentBook.IsFavorite;
                    IsReadCheck = CurrentBook.IsRead;

                    Cover = CurrentBook.Cover != null
                        ? new Cover() { Id = CurrentBook.Cover.Id, Image = CurrentBook.Cover.Image }
                        : null;
                }
            }
        }

        private async Task<Book> ParseBook(string path)
        {
            Book result = null;
            try
            {
                await Task.Run(async () =>
                {
                    var pBook = EbookParserFactory.Create(path).Parse();
                    using var uow = await App.UnitOfWorkFactory.CreateAsync();
                    var book = await pBook.ToBookAsync(uow);
                    book.Path = path;
                    result = book;
                });
            }
            catch (Exception)
            {
                var content = File.ReadAllBytes(path);
                return new Book
                {
                    Collections = new ObservableCollection<UserCollection>(),
                    Format = Path.GetExtension(path),
                    Signature = Signer.ComputeHash(content),
                    Authors = new ObservableCollection<Author>(),
                    Path = path
                };
            }

            return result;
        }

        private async void HandleLoaded()
        {
            CurrentBook = await ParseBook(books[0]);
            TitleText = $"Book 1 of {books.Count}";
            ProceedButtonText = books.Count == 1 ? "SAVE & FINISH" : "SAVE & NEXT";
            CheckDuplicate(CurrentBook);
        }

        private async void HandleAddNewAuthor()
        {
            var name = await DialogCoordinator.Instance.ShowInputAsync(this, "Adding New Author", "Author's name:");

            if (string.IsNullOrWhiteSpace(name))
            {
                return;
            }

            name = name.Trim();

            var newAuthor = new Author { Name = name };
            AuthorsCollection.Add(newAuthor);

            using var uow = await App.UnitOfWorkFactory.CreateAsync();
            await uow.AuthorRepository.CreateAsync(newAuthor);
        }

        private void HandleRevert()
        {
            ClearErrors();
            AuthorsCollection = new ObservableCollection<Author>(CurrentBook.Authors);
            Series = CurrentBook.Series == null
                ? null
                : new BookSeries { Name = CurrentBook.Series.Name, Id = CurrentBook.Series.Id };
            TitleFieldText = CurrentBook.Title;
            SeriesNumberFieldText = CurrentBook.NumberInSeries.ToString();
            IsFavoriteCheck = CurrentBook.IsFavorite;
            IsReadCheck = CurrentBook.IsRead;
            Cover = new Cover() { Id = CurrentBook.Cover.Id, Image = CurrentBook.Cover.Image };
        }

        private void HandleSaveAndNext()
        {
            IsSaving = true;
            Validate();

            if (HasErrors)
            {
                IsSaving = false;
                return;
            }

            var book = CurrentBook;
            _ = Task.Run(async () =>
            {
                using var uow = await App.UnitOfWorkFactory.CreateAsync();

                book.Series = Series;
                if (Series?.Id == 0)
                {
                    await uow.SeriesRepository.CreateAsync(book.Series);
                    book.SeriesId = book.Series.Id;
                }
                else if (Series != null && Series.Id != 0)
                {
                    await uow.SeriesRepository.UpdateAsync(book.Series);
                    book.SeriesId = book.SeriesId;
                }

                if (IsSeriesSelected)
                {
                    if (Regex.IsMatch(SeriesNumberFieldText, @"\d+(\.\d+)?"))
                    {
                        book.NumberInSeries = decimal.Parse(SeriesNumberFieldText);
                    }
                }

                book.Title = TitleFieldText;
                book.IsFavorite = IsFavoriteCheck;
                book.IsRead = IsReadCheck;

                if (Cover?.Id == 0 && Cover.Image != null)
                {
                    book.Cover = Cover;
                    await uow.CoverRepository.CreateAsync(book.Cover);
                    book.CoverId = book.Cover.Id;
                }

                await uow.BookRepository.CreateAsync(book);

                foreach (var author in AuthorsCollection)
                {
                    await uow.AuthorRepository.AddAuthorForBookAsync(author, book.Id);
                }

                uow.Commit();
            });

            NextBook();
            IsSaving = false;
        }

        private void HandleCancel()
        {
            MessengerInstance.Send(new CloseFlyoutMessage());
        }

        private void HandleChangeCoverButton()
        {
            using var dlg = new OpenFileDialog
            {
                Filter = "All files|*.*|jpg|*.jpg|png|*.png",
                CheckFileExists = true,
                CheckPathExists = true,
                FilterIndex = 0,
                Multiselect = false
            };
            var result = dlg.ShowDialog();
            if (result == DialogResult.OK && dlg.FileName != null)
            {
                Cover.Image = ImageOptimizer.ResizeAndFill(File.ReadAllBytes(dlg.FileName));
            }
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

                CurrentBook = await ParseBook(books[++counter]);

                CheckDuplicate(CurrentBook);
            }
        }

        private async void HandleAddExistingAuthor()
        {
            var dialog = new ChooseAuthorDialog
            {
                DataContext = new ChooseAuthorDialogViewModel(AuthorsCollection.Select(oa => oa.Id),
                    x => Application.Current.Dispatcher.Invoke(() => AuthorsCollection.Add(x)))
            };
            await DialogCoordinator.Instance.ShowMetroDialogAsync(this, dialog);
        }

        private async void HandleChooseExistingSeries()
        {
            var dialog = new ChooseSeriesDialog
            {
                DataContext = new ChooseSeriesDialogViewModel(x => Series = x)
            };
            await DialogCoordinator.Instance.ShowMetroDialogAsync(this, dialog);
        }

        private async void HandleCreateNewSeries()
        {
            var name = await DialogCoordinator.Instance.ShowInputAsync(this, "Adding New Series", "Series name:");
            if (!string.IsNullOrWhiteSpace(name))
            {
                name = name.Trim();
                var newSeries = new BookSeries { Name = name };
                Series = newSeries;

                using var uow = await App.UnitOfWorkFactory.CreateAsync();
                await uow.SeriesRepository.CreateAsync(newSeries);
            }
        }

        private void HandleClearSeries()
        {
            Series = null;
        }

        private async void HandleEditSeries()
        {
            Series.Name = await DialogCoordinator.Instance.ShowInputAsync(this, "Edit Series", "Series name:",
                new MetroDialogSettings { DefaultText = Series.Name });
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
