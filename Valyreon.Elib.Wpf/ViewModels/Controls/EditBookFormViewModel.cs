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
using Valyreon.Elib.Domain;
using Valyreon.Elib.Mvvm;
using Valyreon.Elib.Wpf.Messages;
using Valyreon.Elib.Wpf.Models;
using Valyreon.Elib.Wpf.ValidationAttributes;
using Valyreon.Elib.Wpf.ViewModels.Dialogs;
using Application = System.Windows.Application;

namespace Valyreon.Elib.Wpf.ViewModels.Controls
{
    public class EditBookFormViewModel : ViewModelWithValidation
    {
        private ObservableCollection<Author> authorCollection;

        private byte[] coverImage;

        private bool isFavorite;

        private bool isRead;

        private BookSeries series;

        private string seriesNumberFieldText;

        private string titleFieldText;

        private string descriptionFieldText;

        public EditBookFormViewModel(Book book)
        {
            Book = book;
            AuthorsCollection = new ObservableCollection<Author>(Book.Authors);
            UserCollections = new ObservableCollection<UserCollection>(Book.Collections);
            Series = Book.Series == null
                ? null
                : new BookSeries { Name = Book.Series.Name, Id = Book.Series.Id };
            TitleFieldText = Book.Title;
            DescriptionFieldText = Book.Description;
            SeriesNumberFieldText = Book.NumberInSeries.ToString();
            IsFavoriteCheck = Book.IsFavorite;
            IsReadCheck = Book.IsRead;
            Cover = Book.Cover?.Image;
        }

        public Book Book { get; }

        public ICommand AddExistingAuthorButtonCommand => new RelayCommand(HandleAddExistingAuthor);

        public ICommand AddNewAuthorButtonCommand => new RelayCommand(HandleAddNewAuthor);

        [NotEmpty(ErrorMessage = "Book has to have at least one author.")]
        public ObservableCollection<Author> AuthorsCollection
        {
            get => authorCollection;
            private set => Set(() => AuthorsCollection, ref authorCollection, value);
        }

        public ObservableCollection<UserCollection> UserCollections
        {
            get => usersCollections;
            private set => Set(() => UserCollections, ref usersCollections, value);
        }

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

        private ObservableCollection<ObservableEntity> collectionSuggestions;
        private ObservableCollection<UserCollection> usersCollections;

        public ObservableCollection<ObservableEntity> CollectionSuggestions
        {
            get => collectionSuggestions;
            set => Set(() => CollectionSuggestions, ref collectionSuggestions, value);
        }

        public ICommand AddCollectionCommand => new RelayCommand<string>(AddCollection);

        private async void AddCollection(string tag)
        {
            if (string.IsNullOrWhiteSpace(tag))
            {
                return;
            }

            tag = tag.Trim();
            if (UserCollections.Any(c => c.Tag == tag)) // check if book is already in that collection
            {
                return;
            }

            using var uow = await App.UnitOfWorkFactory.CreateAsync();
            var existingCollection = await uow.CollectionRepository.GetByTagAsync(tag);

            if (existingCollection == null)
            {
                UserCollections.Add(new UserCollection { Tag = tag });
            }
            else
            {
                UserCollections.Add(existingCollection);
            }
        }

        public ICommand RefreshSuggestedCollectionsCommand => new RelayCommand<string>(HandleRefreshSuggestedCollections);

        private async void HandleRefreshSuggestedCollections(string token)
        {
            using var uow = await App.UnitOfWorkFactory.CreateAsync();
            var allUserCollections = await uow.CollectionRepository.GetAllAsync();

            var suggestions = allUserCollections.Where(c => !UserCollections.Contains(c) && c.Tag.ToLowerInvariant().Contains(token))
                .Take(4);
            CollectionSuggestions = new ObservableCollection<ObservableEntity>(suggestions.Cast<ObservableEntity>());
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

        public string DescriptionFieldText
        {
            get => descriptionFieldText;
            set => Set(() => DescriptionFieldText, ref descriptionFieldText, value);
        }

        public bool IsSeriesSelected => Series != null;

        public ICommand RemoveAuthorCommand => new RelayCommand<Author>(HandleRemoveAuthor);

        public ICommand RemoveCoverButtonCommand => new RelayCommand(() => Cover = null);

        public ICommand RemoveCollectionCommand => new RelayCommand<UserCollection>(RemoveCollection);

        private void RemoveCollection(UserCollection collection)
        {
            UserCollections.Remove(collection);
        }

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

        [RegularExpression(@"^\d*(\.\d)?$")]
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

        private void HandleAddNewAuthor()
        {
            var viewModel = new SimpleTextInputDialogViewModel("Add New Author", "Author's name:", name =>
            {
                if (string.IsNullOrWhiteSpace(name))
                {
                    return;
                }

                name = name.Trim();
                var newAuthor = new Author { Name = name };
                AuthorsCollection.Add(newAuthor);
            });

            MessengerInstance.Send(new ShowDialogMessage(viewModel));
        }

        private void HandleRemoveAuthor(Author author)
        {
            _ = AuthorsCollection.Remove(author);
        }

        public bool CreateBook()
        {
            Validate();
            if (HasErrors)
            {
                return false;
            }

            var book = Book;

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

                if (Cover != null)
                {
                    book.Cover = new Cover
                    {
                        Image = Cover
                    };
                    await uow.CoverRepository.CreateAsync(book.Cover);
                    book.CoverId = book.Cover.Id;
                }

                await uow.BookRepository.CreateAsync(book);

                foreach (var author in AuthorsCollection)
                {
                    await uow.AuthorRepository.AddAuthorForBookAsync(author, book.Id);
                }

                foreach (var collection in UserCollections)
                {
                    await uow.CollectionRepository.AddCollectionForBookAsync(collection, book.Id);
                }

                uow.Commit();
                MessengerInstance.Send(new RefreshSidePaneCollectionsMessage());
            });

            return true;
        }

        public bool UpdateBook()
        {
            Validate();
            if (HasErrors)
            {
                return false;
            }

            var book = Book;

            Task.Run(async () =>
            {
                using var uow = await App.UnitOfWorkFactory.CreateAsync();

                if ((book.Series == null && Series != null) || (Series != null && book.Series.Id != Series.Id))
                {
                    book.Series = await uow.SeriesRepository.FindAsync(Series.Id);
                    book.SeriesId = book.Series.Id;
                }
                else if (book.Series != null && Series != null && book.Series.Id == Series.Id)
                {
                    var series = book.Series;
                    series.Name = Series.Name;
                    book.Series = series; // to trgger SeriesInfo update in UI
                    await uow.SeriesRepository.UpdateAsync(book.Series);
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
                book.Description = DescriptionFieldText;

                var removedAuthorIds = new List<int>();
                var oldAndNewCommonAuthorIds = AuthorsCollection.Select(a => a.Id).Intersect(book.Authors.Select(a => a.Id));

                foreach (var author in AuthorsCollection.Where(a => !oldAndNewCommonAuthorIds.Contains(a.Id)))
                {
                    await uow.AuthorRepository.AddAuthorForBookAsync(author, book.Id);
                }

                foreach (var author in book.Authors.Where(a => !oldAndNewCommonAuthorIds.Contains(a.Id)))
                {
                    await uow.AuthorRepository.RemoveAuthorForBookAsync(author, book.Id);
                    removedAuthorIds.Add(author.Id);
                }

                book.Authors = AuthorsCollection;

                var removedCollectionIds = new List<int>();
                var oldAndNewCommonCollectionIds = UserCollections.Select(a => a.Id).Intersect(book.Collections.Select(a => a.Id));

                foreach (var collection in UserCollections.Where(a => !oldAndNewCommonCollectionIds.Contains(a.Id)))
                {
                    await uow.CollectionRepository.AddCollectionForBookAsync(collection, book.Id);
                }

                foreach (var collection in book.Collections.Where(a => !oldAndNewCommonCollectionIds.Contains(a.Id)))
                {
                    await uow.CollectionRepository.RemoveCollectionForBookAsync(collection, book.Id);
                    removedCollectionIds.Add(collection.Id);
                }

                Book.Collections = UserCollections;

                if (Cover != null)
                {
                    if (book.Cover == null) // add new
                    {
                        book.Cover = new Cover { Image = Cover };
                        await uow.CoverRepository.CreateAsync(book.Cover);
                        book.CoverId = book.Cover.Id;
                    }
                    else // update
                    {
                        book.Cover.Image = Cover;
                        await uow.CoverRepository.UpdateAsync(book.Cover);
                    }
                }

                await uow.BookRepository.UpdateAsync(book);
                uow.Commit();

                foreach (var id in removedAuthorIds)
                {
                    if (await uow.AuthorRepository.CountBooksByAuthorAsync(id) == 0)
                    {
                        await uow.AuthorRepository.DeleteAsync(id);
                    }
                }

                foreach (var id in removedCollectionIds)
                {
                    if (await uow.CollectionRepository.CountBooksInUserCollectionAsync(id) == 0)
                    {
                        await uow.CollectionRepository.DeleteAsync(id);
                    }
                }

                uow.Commit();
                MessengerInstance.Send(new RefreshSidePaneCollectionsMessage());
            });
            return true;
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
            using var uow = await App.UnitOfWorkFactory.CreateAsync();
            var allAuthors = await uow.AuthorRepository.GetAllAsync();
            var ignoreIds = AuthorsCollection.Select(a => a.Id).ToList();

            var viewModel = new ChooseAuthorDialogViewModel(allAuthors.Where(a => !ignoreIds.Contains(a.Id)),
                    x => Application.Current.Dispatcher.Invoke(() => AuthorsCollection.Add(x)));
            MessengerInstance.Send(new ShowDialogMessage(viewModel));
        }

        private async void HandleChooseExistingSeries()
        {
            using var uow = await App.UnitOfWorkFactory.CreateAsync();
            var allSeries = await uow.SeriesRepository.GetAllAsync();

            var viewModel = new ChooseSeriesDialogViewModel(allSeries, x => Series = x);
            MessengerInstance.Send(new ShowDialogMessage(viewModel));
        }

        private void HandleCreateNewSeries()
        {
            var dialogViewModel = new SimpleTextInputDialogViewModel("Add New Series", "Series name:", name =>
            {
                if (string.IsNullOrWhiteSpace(name))
                {
                    return;
                }

                name = name.Trim();
                var newSeries = new BookSeries { Name = name };
                Series = newSeries;
            });

            MessengerInstance.Send(new ShowDialogMessage(dialogViewModel));
        }

        private void HandleClearSeries()
        {
            Series = null;
        }

        private void HandleEditSeries()
        {
            var dialogViewModel = new SimpleTextInputDialogViewModel("Adding New Series", "Series name:", name =>
            {
                if (string.IsNullOrWhiteSpace(name))
                {
                    return;
                }

                Series.Name = name.Trim();

                _ = Task.Run(async () =>
                {
                    using var uow = await App.UnitOfWorkFactory.CreateAsync();
                    await uow.SeriesRepository.UpdateAsync(Series);
                    uow.Commit();
                });
            });
        }
    }
}
