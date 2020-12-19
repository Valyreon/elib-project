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
using Domain;
using ElibWpf.Messages;
using ElibWpf.Models;
using ElibWpf.ValidationAttributes;
using ElibWpf.ViewModels.Dialogs;
using ElibWpf.Views.Dialogs;
using MahApps.Metro.Controls.Dialogs;
using MVVMLibrary;
using Application = System.Windows.Application;

namespace ElibWpf.ViewModels.Flyouts
{
    public class AddNewBooksViewModel : ViewModelWithValidation
    {
        private readonly IList<Book> books;

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

        public AddNewBooksViewModel(IEnumerable<Book> newBooks)
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

        public Brush SeriesColor
        {
            get
            {
                if (!IsSeriesSelected)
                {
                    return Brushes.Gray;
                }

                return (Brush)new BrushConverter().ConvertFrom("#bbb");
            }
        }

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

                    if (CurrentBook.Cover != null)
                    {
                        Cover = new Cover() { Id = CurrentBook.Cover.Id, Image = CurrentBook.Cover.Image };
                    }
                    else
                    {
                        Cover = null;
                    }
                }
            }
        }

        private void HandleLoaded()
        {
            CurrentBook = books[0];
            TitleText = $"Book 1 of {books.Count}";
            ProceedButtonText = books.Count == 1 ? "SAVE & FINISH" : "SAVE & NEXT";
            CheckDuplicate(CurrentBook);
        }

        private async void HandleAddNewAuthor()
        {
            var name = await DialogCoordinator.Instance.ShowInputAsync(this, "Adding New Author", "Author's name:");
            if (!string.IsNullOrWhiteSpace(name))
            {
                name = name.Trim();

                var newAuthor = new Author { Name = name };
                AuthorsCollection.Add(newAuthor);

                _ = Task.Run(() =>
                {
                    using var uow = App.UnitOfWorkFactory.Create();
                    uow.AuthorRepository.Add(newAuthor);
                });
            }
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

        private async void HandleSaveAndNext()
        {
            IsSaving = true;
            Validate();
            if (!HasErrors)
            {
                var book = CurrentBook;
                await Task.Run(() =>
                {
                    using var uow = App.UnitOfWorkFactory.Create();

                    book.Series = Series;
                    if (Series != null && Series.Id == 0)
                    {
                        uow.SeriesRepository.Add(book.Series);
                        book.SeriesId = book.Series.Id;
                    }
                    else if (Series != null && Series.Id != 0)
                    {
                        uow.SeriesRepository.Update(book.Series);
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

                    uow.RawFileRepository.Add(book.File.RawFile);
                    book.File.RawFileId = book.File.RawFile.Id;
                    uow.EFileRepository.Add(book.File);
                    book.FileId = book.File.Id;

                    if (Cover != null && Cover.Id == 0 && Cover.Image != null)
                    {
                        book.Cover = Cover;
                        uow.CoverRepository.Add(book.Cover);
                        book.CoverId = book.Cover.Id;
                    }

                    uow.BookRepository.Add(book);

                    foreach (var author in AuthorsCollection)
                    {
                        uow.AuthorRepository.AddAuthorForBook(author, book.Id);
                    }

                    uow.Commit();
                });

                NextBook();
            }

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

        private void NextBook()
        {
            if (counter >= books.Count - 1)
            {
                MessengerInstance.Send(new CloseFlyoutMessage());
            }
            else
            {
                ClearErrors();
                TitleText = $"Book {counter + 2} of {books.Count}";
                if (counter == books.Count - 2)
                {
                    ProceedButtonText = "SAVE & FINISH";
                }

                CurrentBook = books[++counter];

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
                var temp = Series;
                Series = newSeries;

                _ = Task.Run(() =>
                {
                    using var uow = App.UnitOfWorkFactory.Create();
                    uow.SeriesRepository.Add(newSeries);
                });
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
            if (uow.EFileRepository.SignatureExists(book.File.Signature))
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
