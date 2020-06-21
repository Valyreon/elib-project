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
using ElibWpf.ValidationAttributes;
using ElibWpf.ViewModels.Dialogs;
using ElibWpf.Views.Dialogs;
using MahApps.Metro.Controls.Dialogs;
using Models;

using MVVMLibrary;
using Application = System.Windows.Application;

namespace ElibWpf.ViewModels.Flyouts
{
    public class EditBookViewModel : ViewModelWithValidation
    {
        private ObservableCollection<Author> authorCollection;

        private byte[] coverImage;

        private bool isFavorite;

        private bool isRead;

        private BookSeries series;

        private string seriesNumberFieldText;

        private string titleFieldText;

        public EditBookViewModel(Book book)
        {
            this.Book = book;
            this.coverImage = book.Cover?.Image;
            this.HandleRevert();
        }

        public ICommand AddExistingAuthorButtonCommand => new RelayCommand(this.HandleAddExistingAuthor);

        public ICommand AddNewAuthorButtonCommand => new RelayCommand(this.HandleAddNewAuthor);

        [NotEmpty(ErrorMessage = "Book has to have at least one author.")]
        public ObservableCollection<Author> AuthorsCollection
        {
            get => this.authorCollection;
            private set { this.Set(() => this.AuthorsCollection, ref this.authorCollection, value); }
        }

        public Book Book { get; }

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

        public bool IsSeriesSelected => this.Series != null;

        public ICommand RemoveAuthorCommand => new RelayCommand<int>(this.HandleRemoveAuthor);

        public ICommand RemoveCoverButtonCommand => new RelayCommand(() => this.Cover = null);

        public ICommand RevertButtonCommand => new RelayCommand(this.HandleRevert);

        public ICommand SaveButtonCommand => new RelayCommand(this.HandleSave);

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

        [Required(ErrorMessage = "Book title can't be empty.")]
        public string TitleFieldText
        {
            get => this.titleFieldText;
            set => this.Set(() => this.TitleFieldText, ref this.titleFieldText, value);
        }

        private async void HandleAddNewAuthor()
        {
            string name = await DialogCoordinator.Instance.ShowInputAsync(this, "Adding New Author", "Author's name:");
            if (string.IsNullOrWhiteSpace(name))
            {
                return;
            }

            name = name.Trim();
            Author newAuthor = new Author { Name = name };
            this.AuthorsCollection.Add(newAuthor);

            _ = Task.Run(() =>
            {
                using var uow = ApplicationSettings.CreateUnitOfWork();
                uow.AuthorRepository.Add(newAuthor);
                uow.Commit();
            });
        }

        private void HandleRemoveAuthor(int id)
        {
            Author obsAuthor = this.AuthorsCollection.FirstOrDefault(c => c.Id == id);
            if (obsAuthor == null)
            {
                return;
            }

            this.AuthorsCollection.Remove(obsAuthor);

            Task.Run(() =>
            {
                using var uow = ApplicationSettings.CreateUnitOfWork();
                if (uow.AuthorRepository.CountBooksByAuthor(id) > 1)
                {
                    return;
                }

                uow.AuthorRepository.Remove(id);
                uow.Commit();
            });
        }

        private void HandleRevert()
        {
            this.ClearErrors();
            this.AuthorsCollection = new ObservableCollection<Author>(this.Book.Authors);
            this.Series = this.Book.Series == null
                ? null
                : new BookSeries { Name = this.Book.Series.Name, Id = this.Book.Series.Id };
            this.TitleFieldText = this.Book.Title;
            this.SeriesNumberFieldText = this.Book.NumberInSeries.ToString();
            this.IsFavoriteCheck = this.Book.IsFavorite;
            this.IsReadCheck = this.Book.IsRead;
            this.Cover = this.Book.Cover?.Image;
        }

        private void HandleSave()
        {
            this.Validate();
            if (this.HasErrors)
            {
                return;
            }

            Book book = this.Book;

            Task.Run(() =>
            {
                using var uow = ApplicationSettings.CreateUnitOfWork();

                if ((book.Series == null && this.Series != null) || (this.Series != null && book.Series.Id != this.Series.Id))
                {
                    book.Series = uow.SeriesRepository.Find(this.Series.Id);
                    book.SeriesId = book.Series.Id;
                }
                else if (book.Series != null && this.Series != null && book.Series.Id == this.Series.Id)
                {
                    var series = book.Series;
                    series.Name = this.Series.Name;
                    book.Series = series; // to trgger SeriesInfo update in UI
                    uow.SeriesRepository.Update(book.Series);
                } else if (book.Series != null && this.Series == null)
                {
                    book.SeriesId = null;
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

                var oldAndNewCommonIds = this.AuthorsCollection.Select(a => a.Id).Intersect(book.Authors.Select(a => a.Id));

                foreach (Author author in this.AuthorsCollection)
                {
                    if(!oldAndNewCommonIds.Contains(author.Id))
                    {
                        uow.AuthorRepository.AddAuthorForBook(author, book.Id);
                    }
                }

                foreach(Author author in book.Authors)
                {
                    if (!oldAndNewCommonIds.Contains(author.Id))
                    {
                        uow.AuthorRepository.RemoveAuthorForBook(author, book.Id);
                    }
                }

                book.Authors = this.AuthorsCollection;

                if (this.Cover != null)
                {
                    if (book.Cover == null) // add new
                    {
                        book.Cover = new Cover { Image = this.Cover };
                        uow.CoverRepository.Add(book.Cover);
                        book.CoverId = book.Cover.Id;
                    }
                    else // update
                    {
                        book.Cover.Image = this.Cover;
                        uow.CoverRepository.Update(book.Cover);
                    }
                }

                uow.BookRepository.Update(book);
                uow.Commit();
            });

            this.MessengerInstance.Send(new ShowBookDetailsMessage(this.Book));
        }

        private void HandleCancel()
        {
            this.MessengerInstance.Send(new ShowBookDetailsMessage(this.Book));
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
            if (string.IsNullOrWhiteSpace(name))
            {
                return;
            }

            name = name.Trim();
            BookSeries newSeries = new BookSeries { Name = name };
            this.Series = newSeries;

            _ = Task.Run(() =>
            {
                using var uow = ApplicationSettings.CreateUnitOfWork();
                uow.SeriesRepository.Add(newSeries);
                uow.Commit();
            });
        }

        private void HandleClearSeries()
        {
            this.Series = null;
        }

        private async void HandleEditSeries()
        {
            this.Series.Name = await DialogCoordinator.Instance.ShowInputAsync(this, "Edit Series", "Series name:",
                new MetroDialogSettings { DefaultText = this.Series.Name });

            _ = Task.Run(() =>
            {
                using var uow = ApplicationSettings.CreateUnitOfWork();
                uow.SeriesRepository.Update(this.Series);
                uow.Commit();
            });
        }
    }
}