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
using System.Windows.Media.Animation;
using DataLayer;
using Domain;
using ElibWpf.Messages;
using ElibWpf.ValidationAttributes;
using ElibWpf.ViewModels.Dialogs;
using ElibWpf.Views.Dialogs;
using MahApps.Metro.Controls.Dialogs;
using Models;
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
            this.books = newBooks.ToList();
        }

        public string AddAuthorFieldText
        {
            get => this.addAuthorFieldText;
            set => this.Set(() => this.AddAuthorFieldText, ref this.addAuthorFieldText, value);
        }

        public ICommand AddExistingAuthorButtonCommand => new RelayCommand(this.HandleAddExistingAuthor);

        public ICommand AddNewAuthorButtonCommand => new RelayCommand(this.HandleAddNewAuthor);

        [NotEmpty(ErrorMessage = "Book has to have at least one author.")]
        public ObservableCollection<Author> AuthorsCollection
        {
            get => this.authorCollection;
            private set { this.Set(() => this.AuthorsCollection, ref this.authorCollection, value); }
        }

        public ICommand CancelButtonCommand => new RelayCommand(this.HandleCancel);

        public ICommand ChangeCoverButtonCommand => new RelayCommand(this.HandleChangeCoverButton);

        public ICommand ChooseExistingSeriesCommand => new RelayCommand(this.HandleChooseExistingSeries);

        public ICommand ClearSeriesCommand => new RelayCommand(this.HandleClearSeries);

        public Cover Cover
        {
            get => this.coverImage;
            set => this.Set(() => this.Cover, ref this.coverImage, value);
        }

        public ICommand CreateNewSeriesCommand => new RelayCommand(this.HandleCreateNewSeries);

        public ICommand EditSeriesCommand => new RelayCommand(this.HandleEditSeries);

        public bool IsCurrentBookDuplicate
        {
            get => this.isSaving || this.isCurrentDuplicate;
            set => this.Set(() => this.IsCurrentBookDuplicate, ref this.isCurrentDuplicate, value);
        }

        public bool IsFavoriteCheck
        {
            get => this.isFavorite;
            set => this.Set(() => this.IsFavoriteCheck, ref this.isFavorite, value);
        }

        public bool IsReadCheck
        {
            get => this.isRead;
            set => this.Set(() => this.IsReadCheck, ref this.isRead, value);
        }

        public bool IsSaving
        {
            get => this.isSaving;
            set
            {
                this.Set(() => this.IsSaving, ref this.isSaving, value);
                this.RaisePropertyChanged(() => this.IsCurrentBookDuplicate);
            }
        }

        public bool IsSeriesSelected => this.Series != null;

        public ICommand LoadedCommand => new RelayCommand(this.HandleLoaded);

        public ICommand NextButtonCommand => new RelayCommand(this.HandleSaveAndNext);

        public string ProceedButtonText
        {
            get => this.proceedButtonText;
            set => this.Set(() => this.ProceedButtonText, ref this.proceedButtonText, value);
        }

        public ICommand RemoveAuthorCommand =>
            new RelayCommand<int>(x => this.AuthorsCollection.Remove(this.AuthorsCollection.FirstOrDefault(c => c.Id == x)));

        public ICommand RemoveCoverButtonCommand => new RelayCommand(() => this.Cover = null);

        public ICommand RevertButtonCommand => new RelayCommand(this.HandleRevert);

        public BookSeries Series
        {
            get => this.series;
            set
            {
                this.Set(() => this.Series, ref this.series, value);
                this.RaisePropertyChanged(() => this.IsSeriesSelected);
                this.RaisePropertyChanged(() => this.SeriesColor);
            }
        }

        public Brush SeriesColor
        {
            get
            {
                if (!this.IsSeriesSelected)
                {
                    return Brushes.Gray;
                }

                return (Brush)new BrushConverter().ConvertFrom("#bbb");
            }
        }

        public string SeriesNumberFieldText
        {
            get => this.seriesNumberFieldText;
            set => this.Set(() => this.SeriesNumberFieldText, ref this.seriesNumberFieldText, value);
        }

        public ICommand SkipButtonCommand => new RelayCommand(this.NextBook);

        [Required(ErrorMessage = "Book title can't be empty.")]
        public string TitleFieldText
        {
            get => this.titleFieldText;
            set => this.Set(() => this.TitleFieldText, ref this.titleFieldText, value);
        }

        public string TitleText
        {
            get => this.titleText;
            set => this.Set(() => this.TitleText, ref this.titleText, value);
        }

        public string WarningText
        {
            get => this.warning;
            set => this.Set(() => this.WarningText, ref this.warning, value);
        }

        private Book CurrentBook
        {
            get => this.currentBook;

            set
            {
                this.currentBook = value;
                this.Set(() => this.CurrentBook, ref this.currentBook, value);
                if (this.currentBook != null)
                {
                    this.AuthorsCollection = new ObservableCollection<Author>(this.CurrentBook.Authors);

                    if (this.CurrentBook.Series != null)
                    {
                        this.Series = new BookSeries() { Id = this.CurrentBook.Series.Id, Name = this.CurrentBook.Series.Name };
                    }
                    else
                    {
                        this.Series = null;
                    }

                    this.TitleFieldText = this.CurrentBook.Title;
                    this.SeriesNumberFieldText = this.CurrentBook.NumberInSeries.ToString();
                    this.IsFavoriteCheck = this.CurrentBook.IsFavorite;
                    this.IsReadCheck = this.CurrentBook.IsRead;

                    if (this.CurrentBook.Cover != null)
                    {
                        this.Cover = new Cover() { Id = this.CurrentBook.Cover.Id, Image = this.CurrentBook.Cover.Image };
                    }
                    else
                    {
                        this.Cover = null;
                    }
                }
            }
        }

        private void HandleLoaded()
        {
            this.CurrentBook = this.books[0];
            this.TitleText = $"Book 1 of {this.books.Count}";
            this.ProceedButtonText = this.books.Count == 1 ? "SAVE & FINISH" : "SAVE & NEXT";
            this.CheckDuplicate(this.CurrentBook);
        }

        private async void HandleAddNewAuthor()
        {
            string name = await DialogCoordinator.Instance.ShowInputAsync(this, "Adding New Author", "Author's name:");
            if (!string.IsNullOrWhiteSpace(name))
            {
                name = name.Trim();

                Author newAuthor = new Author { Name = name };
                this.AuthorsCollection.Add(newAuthor);

                _ = Task.Run(() =>
                {
                    using var uow = ApplicationSettings.CreateUnitOfWork();
                    uow.AuthorRepository.Add(newAuthor);
                });
            }
        }

        private void HandleRevert()
        {
            this.ClearErrors();
            this.AuthorsCollection = new ObservableCollection<Author>(this.CurrentBook.Authors);
            this.Series = this.CurrentBook.Series == null
                ? null
                : new BookSeries { Name = this.CurrentBook.Series.Name, Id = this.CurrentBook.Series.Id };
            this.TitleFieldText = this.CurrentBook.Title;
            this.SeriesNumberFieldText = this.CurrentBook.NumberInSeries.ToString();
            this.IsFavoriteCheck = this.CurrentBook.IsFavorite;
            this.IsReadCheck = this.CurrentBook.IsRead;
            this.Cover = new Cover() { Id = this.CurrentBook.Cover.Id, Image = this.CurrentBook.Cover.Image };
        }

        private async void HandleSaveAndNext()
        {
            this.IsSaving = true;
            this.Validate();
            if (!this.HasErrors)
            {
                Book book = this.CurrentBook;
                await Task.Run(() =>
                {
                    using var uow = ApplicationSettings.CreateUnitOfWork();

                    book.Series = this.Series;
                    if (this.Series != null && this.Series.Id == 0)
                    {
                        uow.SeriesRepository.Add(book.Series);
                        book.SeriesId = book.Series.Id;
                    }
                    else if (this.Series != null && this.Series.Id != 0)
                    {
                        uow.SeriesRepository.Update(book.Series);
                        book.SeriesId = book.SeriesId;
                    }

                    if (this.IsSeriesSelected)
                    {
                        if (Regex.IsMatch(this.SeriesNumberFieldText, @"\d+(\.\d+)?"))
                        {
                            book.NumberInSeries = decimal.Parse(this.SeriesNumberFieldText);
                        }
                    }

                    book.Title = this.TitleFieldText;
                    book.IsFavorite = this.IsFavoriteCheck;
                    book.IsRead = this.IsReadCheck;

                    uow.RawFileRepository.Add(book.File.RawFile);
                    book.File.RawFileId = book.File.RawFile.Id;
                    uow.EFileRepository.Add(book.File);
                    book.FileId = book.File.Id;

                    if (this.Cover != null && Cover.Id == 0 && Cover.Image != null)
                    {
                        book.Cover = this.Cover;
                        uow.CoverRepository.Add(book.Cover);
                        book.CoverId = book.Cover.Id;
                    }

                    uow.BookRepository.Add(book);

                    foreach (Author author in this.AuthorsCollection)
                    {
                        uow.AuthorRepository.AddAuthorForBook(author, book.Id);
                    }

                    uow.Commit();
                });

                this.NextBook();
            }

            this.IsSaving = false;
        }

        private void HandleCancel()
        {
            this.MessengerInstance.Send(new CloseFlyoutMessage());
        }

        private void HandleChangeCoverButton()
        {
            using OpenFileDialog dlg = new OpenFileDialog
            {
                Filter = "All files|*.*|jpg|*.jpg|png|*.png",
                CheckFileExists = true,
                CheckPathExists = true,
                FilterIndex = 0,
                Multiselect = false
            };
            DialogResult result = dlg.ShowDialog();
            if (result == DialogResult.OK && dlg.FileName != null)
            {
                this.Cover.Image = ImageOptimizer.ResizeAndFill(File.ReadAllBytes(dlg.FileName));
            }
        }

        private void NextBook()
        {
            if (this.counter >= this.books.Count - 1)
            {
                this.MessengerInstance.Send(new CloseFlyoutMessage());
            }
            else
            {
                this.ClearErrors();
                this.TitleText = $"Book {this.counter + 2} of {this.books.Count}";
                if (this.counter == this.books.Count - 2)
                {
                    this.ProceedButtonText = "SAVE & FINISH";
                }

                this.CurrentBook = this.books[++this.counter];

                this.CheckDuplicate(this.CurrentBook);
            }
        }

        private async void HandleAddExistingAuthor()
        {
            ChooseAuthorDialog dialog = new ChooseAuthorDialog
            {
                DataContext = new ChooseAuthorDialogViewModel(this.AuthorsCollection.Select(oa => oa.Id),
                    x => Application.Current.Dispatcher.Invoke(() => this.AuthorsCollection.Add(x)))
            };
            await DialogCoordinator.Instance.ShowMetroDialogAsync(this, dialog);
        }

        private async void HandleChooseExistingSeries()
        {
            ChooseSeriesDialog dialog = new ChooseSeriesDialog
            {
                DataContext = new ChooseSeriesDialogViewModel(x =>
                {
                    this.Series = x;
                })
            };
            await DialogCoordinator.Instance.ShowMetroDialogAsync(this, dialog);
        }

        private async void HandleCreateNewSeries()
        {
            string name = await DialogCoordinator.Instance.ShowInputAsync(this, "Adding New Series", "Series name:");
            if (!string.IsNullOrWhiteSpace(name))
            {
                using var uow = ApplicationSettings.CreateUnitOfWork();
                name = name.Trim();
                BookSeries newSeries = new BookSeries { Name = name };
                BookSeries temp = this.Series;
                this.Series = newSeries;
                uow.SeriesRepository.CleanSeries(temp.Id);

                _ = Task.Run(() =>
                {
                    using var uow = ApplicationSettings.CreateUnitOfWork();
                    uow.SeriesRepository.Add(newSeries);
                });
            }
        }

        private void HandleClearSeries()
        {
            BookSeries temp = this.Series;
            this.Series = null;
            using var uow = ApplicationSettings.CreateUnitOfWork();
            uow.SeriesRepository.CleanSeries(temp.Id);
        }

        private async void HandleEditSeries()
        {
            this.Series.Name = await DialogCoordinator.Instance.ShowInputAsync(this, "Edit Series", "Series name:",
                new MetroDialogSettings { DefaultText = this.Series.Name });
        }

        private void CheckDuplicate(Book book)
        {
            using var uow = ApplicationSettings.CreateUnitOfWork();
            if (uow.EFileRepository.SignatureExists(book.File.Signature))
            {
                this.WarningText = "This book is a duplicate of a book already in the database.";
                this.IsCurrentBookDuplicate = true;
            }
            else
            {
                this.WarningText = null;
                this.IsCurrentBookDuplicate = false;
            }
        }
    }
}