using DataLayer;
using Domain;
using ElibWpf.Messages;
using ElibWpf.ValidationAttributes;
using ElibWpf.ViewModels.Dialogs;
using ElibWpf.Views.Dialogs;
using GalaSoft.MvvmLight.Command;
using MahApps.Metro.Controls.Dialogs;
using Models;
using Models.Observables;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;

namespace ElibWpf.ViewModels.Flyouts
{
    public class AddNewBooksViewModel : ViewModelWithValidation
    {
        private readonly IList<ObservableBook> books;
        private int counter = 0;

        private ObservableBook currentBook;

        private ObservableBook CurrentBook
        {
            get => currentBook;

            set
            {
                currentBook = value;
                Set(() => CurrentBook, ref currentBook, value);
                if(currentBook != null)
                {
                    AuthorsCollection = new ObservableCollection<ObservableAuthor>(CurrentBook.Authors);
                    if (CurrentBook.Series != null)
                        Series = new ObservableSeries(new BookSeries { Id = CurrentBook.Series.Id, Name = CurrentBook.Series.Name });
                    else
                        Series = null;
                    TitleFieldText = CurrentBook.Title;
                    SeriesNumberFieldText = CurrentBook.NumberInSeries.ToString();
                    IsFavoriteCheck = CurrentBook.IsFavorite;
                    IsReadCheck = CurrentBook.IsRead;
                    Cover = CurrentBook.Cover;
                }
            }
        }

        public AddNewBooksViewModel(IList<Book> newBooks)
        {
            books = newBooks.Select(b => new ObservableBook(b)).ToList();
        }

        public ICommand LoadedCommand { get => new RelayCommand(this.HandleLoaded); }

        private List<ObservableBook> duplicates = new List<ObservableBook>();
        private void HandleLoaded()
        {
            CurrentBook = books[0];
            TitleText = $"Book 1 of {books.Count}";
            ProceedButtonText = books.Count == 1 ? "SAVE & FINISH" : "SAVE & NEXT";
            CheckDuplicate(CurrentBook);
        }

        private ObservableCollection<ObservableAuthor> authorCollection;
        [NotEmpty(ErrorMessage = "Book has to have at least one author.")]
        public ObservableCollection<ObservableAuthor> AuthorsCollection
        {
            get => authorCollection;
            private set
            {
                Set(() => AuthorsCollection, ref authorCollection, value);
            }
        }

        private string titleFieldText;

        [Required(ErrorMessage = "Book title can't be empty.")]
        public string TitleFieldText
        {
            get => titleFieldText;
            set => Set(() => TitleFieldText, ref titleFieldText, value);
        }

        private string warning = null;
        public string WarningText
        {
            get => warning;
            set => Set(() => WarningText, ref warning, value);
        }

        private string seriesNumberFieldText;

        public string SeriesNumberFieldText
        {
            get => seriesNumberFieldText;
            set => Set(() => SeriesNumberFieldText, ref seriesNumberFieldText, value);
        }

        private string titleText;

        public string TitleText
        {
            get => titleText;
            set => Set(() => TitleText, ref titleText, value);
        }

        private string proceedButtonText;

        public string ProceedButtonText
        {
            get => proceedButtonText;
            set => Set(() => ProceedButtonText, ref proceedButtonText, value);
        }

        private string addAuthorFieldText;

        public string AddAuthorFieldText
        {
            get => addAuthorFieldText;
            set => Set(() => AddAuthorFieldText, ref addAuthorFieldText, value);
        }

        private bool isRead;

        public bool IsReadCheck
        {
            get => isRead;
            set => Set(() => IsReadCheck, ref isRead, value);
        }

        private bool isCurrentDuplicate = true;
        public bool IsCurrentBookDuplicate
        {
            get => isSaving || isCurrentDuplicate;
            set => Set(() => IsCurrentBookDuplicate, ref isCurrentDuplicate, value);
        }

        private bool isFavorite;

        public bool IsFavoriteCheck
        {
            get => isFavorite;
            set => Set(() => IsFavoriteCheck, ref isFavorite, value);
        }

        private byte[] coverImage;

        public byte[] Cover
        {
            get => coverImage;
            set => Set(() => Cover, ref coverImage, value);
        }

        public bool isSaving;
        public bool IsSaving
        {
            get => isSaving;
            set
            {
                Set(() => IsSaving, ref isSaving, value);
                RaisePropertyChanged(() => IsCurrentBookDuplicate);
            }
        }

        private ObservableSeries series;
        public ObservableSeries Series
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
                    return Brushes.Gray;
                return (Brush)(new BrushConverter().ConvertFrom("#bbb"));
            }
        }

        public bool IsSeriesSelected { get => Series != null; }

        public ICommand AddNewAuthorButtonCommand { get => new RelayCommand(this.HandleAddNewAuthor); }

        private async void HandleAddNewAuthor()
        {
            string name = await DialogCoordinator.Instance.ShowInputAsync(this, "Adding New Author", "Author's name:");
            if (!string.IsNullOrWhiteSpace(name))
            {
                name = name.Trim();
                if (AuthorsCollection.Where(c => c.Name == name).Any()) // check if book is already in that collection
                {
                    MessengerInstance.Send(new ShowDialogMessage("", $"This author is already added."));
                }
                else // if not
                {
                    Author newAuthor = new Author { Name = name };
                    AuthorsCollection.Add(new ObservableAuthor(newAuthor));

                    _ = Task.Run(() =>
                    {
                        using ElibContext database = ApplicationSettings.CreateContext();
                        var existingAuthor = database.Authors.Where(c => c.Name == name).FirstOrDefault();
                        if (existingAuthor != null)
                        {
                            newAuthor.Id = existingAuthor.Id;
                        }
                    });
                }
            }
        }

        public ICommand RemoveAuthorCommand { get => new RelayCommand<int>((x) => AuthorsCollection.Remove(AuthorsCollection.Where(c => c.Id == x).FirstOrDefault())); }

        public ICommand RevertButtonCommand { get => new RelayCommand(this.HandleRevert); }

        private void HandleRevert()
        {
            this.ClearErrors();
            AuthorsCollection = new ObservableCollection<ObservableAuthor>(CurrentBook.Authors);
            Series = CurrentBook.Series == null ? null : new ObservableSeries(new BookSeries { Name = CurrentBook.Series.Name, Id = CurrentBook.Series.Id });
            TitleFieldText = CurrentBook.Title;
            SeriesNumberFieldText = CurrentBook.NumberInSeries.ToString();
            IsFavoriteCheck = CurrentBook.IsFavorite;
            IsReadCheck = CurrentBook.IsRead;
            Cover = CurrentBook.Cover;
        }

        public ICommand NextButtonCommand { get => new RelayCommand(this.HandleSaveAndNext); }

        private async void HandleSaveAndNext()
        {
            IsSaving = true;
            this.Validate();
            if (!this.HasErrors)
            {
                var Book = CurrentBook;
                await Task.Run(() =>
                {
                    using ElibContext database = ApplicationSettings.CreateContext();


                    if ((Book.Series == null && Series != null) || (Series != null && Book.Series.Id != Series.Id))
                    {
                        Book.Series = new ObservableSeries(database.Series.Where(s => s.Id == Series.Id).FirstOrDefault());
                    }
                    else if (Series != null)
                    {
                        Book.Series.Name = Series.Name;
                    }
                    else
                    {
                        Book.Series = null;
                    }

                    if (IsSeriesSelected)
                    {
                        if (Regex.IsMatch(SeriesNumberFieldText, @"\d+(\.\d+)?"))
                            Book.NumberInSeries = decimal.Parse(SeriesNumberFieldText);
                    }

                    Book.Title = TitleFieldText;
                    Book.IsFavorite = IsFavoriteCheck;
                    Book.IsRead = IsReadCheck;
                    Book.Authors.Clear();
                    foreach (var author in AuthorsCollection)
                        Book.Authors.Add(author);
                    Book.Cover = Cover;

                    database.Books.Add(Book.Book);
                    database.SaveChanges();
                });

                NextBook();
            }
            IsSaving = false;
        }

        public ICommand CancelButtonCommand { get => new RelayCommand(this.HandleCancel); }

        private void HandleCancel()
        {
            MessengerInstance.Send(new CloseFlyoutMessage());
        }

        public ICommand ChangeCoverButtonCommand { get => new RelayCommand(this.HandleChangeCoverButton); }

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
            var result = dlg.ShowDialog();
            if (result == DialogResult.OK && dlg.FileName != null)
            {
                this.Cover = ImageOptimizer.ResizeAndFill(File.ReadAllBytes(dlg.FileName));
            }
        }

        public ICommand RemoveCoverButtonCommand { get => new RelayCommand(() => this.Cover = null); }

        public ICommand SkipButtonCommand { get => new RelayCommand(this.NextBook); }

        private void NextBook()
        {
            if (counter >= books.Count - 1)
            {
                MessengerInstance.Send(new CloseFlyoutMessage());
            }
            else
            {
                this.ClearErrors();
                TitleText = $"Book {counter + 2} of {books.Count}";
                if (counter == books.Count - 2)
                {
                    ProceedButtonText = "SAVE & FINISH";
                }

                CurrentBook = books[++counter];
                CheckDuplicate(CurrentBook);
            }
        }

        public ICommand AddExistingAuthorButtonCommand { get => new RelayCommand(this.HandleAddExistingAuthor); }

        private async void HandleAddExistingAuthor()
        {
            var dialog = new ChooseAuthorDialog();
            dialog.DataContext = new ChooseAuthorDialogViewModel(
                this.AuthorsCollection.Select(oa => oa.Author.Id),
                x => App.Current.Dispatcher.Invoke(() => AuthorsCollection.Add(new ObservableAuthor(x))));
            await DialogCoordinator.Instance.ShowMetroDialogAsync(this, dialog);
        }

        public ICommand ChooseExistingSeriesCommand { get => new RelayCommand(this.HandleChooseExistingSeries); }

        private async void HandleChooseExistingSeries()
        {
            var dialog = new ChooseSeriesDialog();
            dialog.DataContext = new ChooseSeriesDialogViewModel(x =>
            {
                if (Series != null && x.Id != Series.Id)
                    CleanSeries(Series.Series);
                Series = new ObservableSeries(x);
            });
            await DialogCoordinator.Instance.ShowMetroDialogAsync(this, dialog);
        }

        private void CleanSeries(BookSeries x)
        {
            if (x != null)
            {
                Task.Run(() =>
                {
                    using ElibContext database = ApplicationSettings.CreateContext();
                    if (database.Books.Where(b => b.SeriesId == x.Id).Count() <= 1)
                    {
                        database.Series.Attach(x);
                        database.Entry(x).State = EntityState.Deleted;
                        database.SaveChanges();
                    }
                });
            }
        }

        public ICommand CreateNewSeriesCommand { get => new RelayCommand(this.HandleCreateNewSeries); }

        private async void HandleCreateNewSeries()
        {
            string name = await DialogCoordinator.Instance.ShowInputAsync(this, "Adding New Series", "Series name:");
            if (!string.IsNullOrWhiteSpace(name))
            {
                name = name.Trim();
                BookSeries newAuthor = new BookSeries { Name = name };
                BookSeries temp = Series?.Series;
                Series = new ObservableSeries(newAuthor);
                CleanSeries(temp);

                _ = Task.Run(() =>
                {
                    using ElibContext database = ApplicationSettings.CreateContext();
                    database.Series.Add(newAuthor);
                    database.SaveChanges();
                });
            }
        }

        public ICommand ClearSeriesCommand { get => new RelayCommand(this.HandleClearSeries); }

        private void HandleClearSeries()
        {
            BookSeries temp = Series?.Series;
            this.Series = null;
            CleanSeries(temp);
        }

        public ICommand EditSeriesCommand { get => new RelayCommand(this.HandleEditSeries); }

        private async void HandleEditSeries()
        {
            Series.Name = await DialogCoordinator.Instance.ShowInputAsync(this, "Edit Series", "Series name:", new MetroDialogSettings { DefaultText = Series.Name });
        }

        private void CheckDuplicate(ObservableBook book)
        {
            using ElibContext database = ApplicationSettings.CreateContext();
            if (database.BookFiles.Where(ef => ef.Signature == book.Book.File.Signature).Count() > 0)
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