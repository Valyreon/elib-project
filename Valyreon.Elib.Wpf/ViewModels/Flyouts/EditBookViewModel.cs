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
using Valyreon.Elib.Mvvm;
using Valyreon.Elib.Wpf.Messages;
using Valyreon.Elib.Wpf.Models;
using Valyreon.Elib.Wpf.ValidationAttributes;
using Valyreon.Elib.Wpf.ViewModels.Dialogs;
using Valyreon.Elib.Wpf.Views.Dialogs;
using Application = System.Windows.Application;

namespace Valyreon.Elib.Wpf.ViewModels.Flyouts
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
            Book = book;
            coverImage = book.Cover?.Image;
            HandleRevert();
        }

        public ICommand AddExistingAuthorButtonCommand => new RelayCommand(HandleAddExistingAuthor);

        public ICommand AddNewAuthorButtonCommand => new RelayCommand(HandleAddNewAuthor);

        [NotEmpty(ErrorMessage = "Book has to have at least one author.")]
        public ObservableCollection<Author> AuthorsCollection
        {
            get => authorCollection;
            private set => Set(() => AuthorsCollection, ref authorCollection, value);
        }

        public Book Book { get; }

        public ICommand CancelButtonCommand => new RelayCommand(HandleCancel);

        public ICommand ChangeCoverButtonCommand => new RelayCommand(HandleChangeCoverButton);

        public ICommand ChooseExistingSeriesCommand => new RelayCommand(HandleChooseExistingSeries);

        public ICommand ClearSeriesCommand => new RelayCommand(HandleClearSeries);

        public byte[] Cover
        {
            get => coverImage;
            set => Set(() => Cover, ref coverImage, value);
        }

        public ICommand CreateNewSeriesCommand => new RelayCommand(HandleCreateNewSeries);

        public ICommand EditSeriesCommand => new RelayCommand(HandleEditSeries);

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

        public bool IsSeriesSelected => Series != null;

        public ICommand RemoveAuthorCommand => new RelayCommand<int>(HandleRemoveAuthor);

        public ICommand RemoveCoverButtonCommand => new RelayCommand(() => Cover = null);

        public ICommand RevertButtonCommand => new RelayCommand(HandleRevert);

        public ICommand SaveButtonCommand => new RelayCommand(HandleSave);

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
                return !IsSeriesSelected
                    ? Brushes.Gray
                    : (Brush)new BrushConverter().ConvertFrom("#bbb");
            }
        }

        public string SeriesNumberFieldText
        {
            get => seriesNumberFieldText;
            set => Set(() => SeriesNumberFieldText, ref seriesNumberFieldText, value);
        }

        [Required(ErrorMessage = "Book title can't be empty.")]
        public string TitleFieldText
        {
            get => titleFieldText;
            set => Set(() => TitleFieldText, ref titleFieldText, value);
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

            _ = Task.Run(() =>
            {
                using var uow = App.UnitOfWorkFactory.Create();
                uow.AuthorRepository.Add(newAuthor);
                uow.Commit();
            });
        }

        private void HandleRemoveAuthor(int id)
        {
            var obsAuthor = AuthorsCollection.FirstOrDefault(c => c.Id == id);
            if (obsAuthor == null)
            {
                return;
            }

            AuthorsCollection.Remove(obsAuthor);

            Task.Run(() =>
            {
                using var uow = App.UnitOfWorkFactory.Create();
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
            ClearErrors();
            AuthorsCollection = new ObservableCollection<Author>(Book.Authors);
            Series = Book.Series == null
                ? null
                : new BookSeries { Name = Book.Series.Name, Id = Book.Series.Id };
            TitleFieldText = Book.Title;
            SeriesNumberFieldText = Book.NumberInSeries.ToString();
            IsFavoriteCheck = Book.IsFavorite;
            IsReadCheck = Book.IsRead;
            Cover = Book.Cover?.Image;
        }

        private void HandleSave()
        {
            Validate();
            if (HasErrors)
            {
                return;
            }

            var book = Book;

            Task.Run(() =>
            {
                using var uow = App.UnitOfWorkFactory.Create();

                if ((book.Series == null && Series != null) || (Series != null && book.Series.Id != Series.Id))
                {
                    book.Series = uow.SeriesRepository.Find(Series.Id);
                    book.SeriesId = book.Series.Id;
                }
                else if (book.Series != null && Series != null && book.Series.Id == Series.Id)
                {
                    var series = book.Series;
                    series.Name = Series.Name;
                    book.Series = series; // to trgger SeriesInfo update in UI
                    uow.SeriesRepository.Update(book.Series);
                }
                else if (book.Series != null && Series == null)
                {
                    book.SeriesId = null;
                    book.Series = null;
                }

                if (IsSeriesSelected)
                {
                    if (Regex.IsMatch(SeriesNumberFieldText, @"\d+(\.\d+)?"))
                    {
                        book.NumberInSeries = decimal.Parse(SeriesNumberFieldText);
                    }
                }
                else
                {
                    book.NumberInSeries = null;
                }

                book.Title = TitleFieldText;
                book.IsFavorite = IsFavoriteCheck;
                book.IsRead = IsReadCheck;

                var oldAndNewCommonIds = AuthorsCollection.Select(a => a.Id).Intersect(book.Authors.Select(a => a.Id));

                foreach (var author in AuthorsCollection)
                {
                    if (!oldAndNewCommonIds.Contains(author.Id))
                    {
                        uow.AuthorRepository.AddAuthorForBook(author, book.Id);
                    }
                }

                foreach (var author in book.Authors)
                {
                    if (!oldAndNewCommonIds.Contains(author.Id))
                    {
                        uow.AuthorRepository.RemoveAuthorForBook(author, book.Id);
                    }
                }

                book.Authors = AuthorsCollection;

                if (Cover != null)
                {
                    if (book.Cover == null) // add new
                    {
                        book.Cover = new Cover { Image = Cover };
                        uow.CoverRepository.Add(book.Cover);
                        book.CoverId = book.Cover.Id;
                    }
                    else // update
                    {
                        book.Cover.Image = Cover;
                        uow.CoverRepository.Update(book.Cover);
                    }
                }

                uow.BookRepository.Update(book);
                uow.Commit();
            });

            MessengerInstance.Send(new ShowBookDetailsMessage(Book));
        }

        private void HandleCancel()
        {
            MessengerInstance.Send(new ShowBookDetailsMessage(Book));
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
                Cover = ImageOptimizer.ResizeAndFill(File.ReadAllBytes(dlg.FileName));
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
            if (string.IsNullOrWhiteSpace(name))
            {
                return;
            }

            name = name.Trim();
            var newSeries = new BookSeries { Name = name };
            Series = newSeries;

            _ = Task.Run(() =>
            {
                using var uow = App.UnitOfWorkFactory.Create();
                uow.SeriesRepository.Add(newSeries);
                uow.Commit();
            });
        }

        private void HandleClearSeries()
        {
            Series = null;
        }

        private async void HandleEditSeries()
        {
            Series.Name = await DialogCoordinator.Instance.ShowInputAsync(this, "Edit Series", "Series name:",
                new MetroDialogSettings { DefaultText = Series.Name });

            _ = Task.Run(() =>
            {
                using var uow = App.UnitOfWorkFactory.Create();
                uow.SeriesRepository.Update(Series);
                uow.Commit();
            });
        }
    }
}