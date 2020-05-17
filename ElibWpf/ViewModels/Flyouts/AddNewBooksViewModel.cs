using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
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
using Application = System.Windows.Application;

namespace ElibWpf.ViewModels.Flyouts
{
    public class AddNewBooksViewModel : ViewModelWithValidation
    {
        private readonly IList<ObservableBook> books;

        private string addAuthorFieldText;

        private ObservableCollection<ObservableAuthor> authorCollection;

        private int counter;

        private byte[] coverImage;

        private ObservableBook currentBook;

        private bool isCurrentDuplicate = true;

        private bool isFavorite;

        private bool isRead;

        private bool isSaving;

        private string proceedButtonText;

        private ObservableSeries series;

        private string seriesNumberFieldText;

        private string titleFieldText;

        private string titleText;

        private string warning;

        public AddNewBooksViewModel(IEnumerable<Book> newBooks)
        {
            this.books = newBooks.Select(b => new ObservableBook(b)).ToList();
        }

        public string AddAuthorFieldText
        {
            get => this.addAuthorFieldText;
            set => this.Set(() => this.AddAuthorFieldText, ref this.addAuthorFieldText, value);
        }

        public ICommand AddExistingAuthorButtonCommand => new RelayCommand(this.HandleAddExistingAuthor);

        public ICommand AddNewAuthorButtonCommand => new RelayCommand(this.HandleAddNewAuthor);

        [NotEmpty(ErrorMessage = "Book has to have at least one author.")]
        public ObservableCollection<ObservableAuthor> AuthorsCollection
        {
            get => this.authorCollection;
            private set { this.Set(() => this.AuthorsCollection, ref this.authorCollection, value); }
        }

        public ICommand CancelButtonCommand => new RelayCommand(this.HandleCancel);

        public ICommand ChangeCoverButtonCommand => new RelayCommand(this.HandleChangeCoverButton);

        public ICommand ChooseExistingSeriesCommand => new RelayCommand(this.HandleChooseExistingSeries);

        public ICommand ClearSeriesCommand => new RelayCommand(this.HandleClearSeries);

        public byte[] Cover
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

        public ObservableSeries Series
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

                return (Brush) new BrushConverter().ConvertFrom("#bbb");
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

        private ObservableBook CurrentBook
        {
            get => this.currentBook;

            set
            {
                this.currentBook = value;
                this.Set(() => this.CurrentBook, ref this.currentBook, value);
                if (this.currentBook != null)
                {
                    this.AuthorsCollection = new ObservableCollection<ObservableAuthor>(this.CurrentBook.Authors);
                    if (this.CurrentBook.Series != null)
                    {
                        this.Series = new ObservableSeries(new BookSeries
                            {Id = this.CurrentBook.Series.Id, Name = this.CurrentBook.Series.Name});
                    }
                    else
                    {
                        this.Series = null;
                    }

                    this.TitleFieldText = this.CurrentBook.Title;
                    this.SeriesNumberFieldText = this.CurrentBook.NumberInSeries.ToString();
                    this.IsFavoriteCheck = this.CurrentBook.IsFavorite;
                    this.IsReadCheck = this.CurrentBook.IsRead;
                    this.Cover = this.CurrentBook.Cover;
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
                if (this.AuthorsCollection.Any(c => c.Name == name)) // check if book is already in that collection
                {
                    this.MessengerInstance.Send(new ShowDialogMessage("", "This author is already added."));
                }
                else // if not
                {
                    Author newAuthor = new Author {Name = name};
                    this.AuthorsCollection.Add(new ObservableAuthor(newAuthor));

                    _ = Task.Run(() =>
                    {
                        using ElibContext database = ApplicationSettings.CreateContext();
                        Author existingAuthor = database.Authors.FirstOrDefault(c => c.Name == name);
                        if (existingAuthor != null)
                        {
                            newAuthor.Id = existingAuthor.Id;
                        }
                    });
                }
            }
        }

        private void HandleRevert()
        {
            this.ClearErrors();
            this.AuthorsCollection = new ObservableCollection<ObservableAuthor>(this.CurrentBook.Authors);
            this.Series = this.CurrentBook.Series == null
                ? null
                : new ObservableSeries(new BookSeries {Name = this.CurrentBook.Series.Name, Id = this.CurrentBook.Series.Id});
            this.TitleFieldText = this.CurrentBook.Title;
            this.SeriesNumberFieldText = this.CurrentBook.NumberInSeries.ToString();
            this.IsFavoriteCheck = this.CurrentBook.IsFavorite;
            this.IsReadCheck = this.CurrentBook.IsRead;
            this.Cover = this.CurrentBook.Cover;
        }

        private async void HandleSaveAndNext()
        {
            this.IsSaving = true;
            this.Validate();
            if (!this.HasErrors)
            {
                ObservableBook book = this.CurrentBook;
                await Task.Run(() =>
                {
                    using ElibContext database = ApplicationSettings.CreateContext();


                    if (book.Series != null &&
                        (book.Series == null && this.Series != null || this.Series != null && book.Series.Id != this.Series.Id))
                    {
                        book.Series =
                            new ObservableSeries(database.Series.FirstOrDefault(s => s.Id == this.Series.Id));
                    }
                    else if (this.Series != null)
                    {
                        if (book.Series != null)
                        {
                            book.Series.Name = this.Series.Name;
                        }
                    }
                    else
                    {
                        book.Series = null;
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
                    book.Authors.Clear();
                    foreach (ObservableAuthor author in this.AuthorsCollection)
                    {
                        book.Authors.Add(author);
                    }

                    book.Cover = this.Cover;

                    database.Books.Add(book.Book);
                    database.SaveChanges();
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
                this.Cover = ImageOptimizer.ResizeAndFill(File.ReadAllBytes(dlg.FileName));
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
                DataContext = new ChooseAuthorDialogViewModel(this.AuthorsCollection.Select(oa => oa.Author.Id),
                    x => Application.Current.Dispatcher.Invoke(() => this.AuthorsCollection.Add(new ObservableAuthor(x))))
            };
            await DialogCoordinator.Instance.ShowMetroDialogAsync(this, dialog);
        }

        private async void HandleChooseExistingSeries()
        {
            ChooseSeriesDialog dialog = new ChooseSeriesDialog
            {
                DataContext = new ChooseSeriesDialogViewModel(x =>
                {
                    if (this.Series != null && x.Id != this.Series.Id)
                    {
                        this.CleanSeries(this.Series.Series);
                    }

                    this.Series = new ObservableSeries(x);
                })
            };
            await DialogCoordinator.Instance.ShowMetroDialogAsync(this, dialog);
        }

        private void CleanSeries(BookSeries x)
        {
            if (x != null)
            {
                Task.Run(() =>
                {
                    using ElibContext database = ApplicationSettings.CreateContext();
                    if (database.Books.Count(b => b.SeriesId == x.Id) <= 1)
                    {
                        database.Series.Attach(x);
                        database.Entry(x).State = EntityState.Deleted;
                        database.SaveChanges();
                    }
                });
            }
        }

        private async void HandleCreateNewSeries()
        {
            string name = await DialogCoordinator.Instance.ShowInputAsync(this, "Adding New Series", "Series name:");
            if (!string.IsNullOrWhiteSpace(name))
            {
                name = name.Trim();
                BookSeries newAuthor = new BookSeries {Name = name};
                BookSeries temp = this.Series?.Series;
                this.Series = new ObservableSeries(newAuthor);
                this.CleanSeries(temp);

                _ = Task.Run(() =>
                {
                    using ElibContext database = ApplicationSettings.CreateContext();
                    database.Series.Add(newAuthor);
                    database.SaveChanges();
                });
            }
        }

        private void HandleClearSeries()
        {
            BookSeries temp = this.Series?.Series;
            this.Series = null;
            this.CleanSeries(temp);
        }

        private async void HandleEditSeries()
        {
            this.Series.Name = await DialogCoordinator.Instance.ShowInputAsync(this, "Edit Series", "Series name:",
                new MetroDialogSettings {DefaultText = this.Series.Name});
        }

        private void CheckDuplicate(ObservableBook book)
        {
            using ElibContext database = ApplicationSettings.CreateContext();
            if (database.BookFiles.Any(ef => ef.Signature == book.Book.File.Signature))
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