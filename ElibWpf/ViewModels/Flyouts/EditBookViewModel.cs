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
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;

namespace ElibWpf.ViewModels.Flyouts
{
    public class EditBookViewModel : ViewModelWithValidation
    {
        public ObservableBook Book { get; private set; }

        public EditBookViewModel(ObservableBook book)
        {
            this.Book = book;
            HandleRevert();
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

        public System.Windows.Media.Brush SeriesColor
        {
            get
            {
                if (!IsSeriesSelected)
                    return System.Windows.Media.Brushes.Gray;
                return (System.Windows.Media.Brush)(new BrushConverter().ConvertFrom("#bbb"));
            }
        }

        private string titleFieldText;

        [Required(ErrorMessage = "Book title can't be empty.")]
        public string TitleFieldText
        {
            get => titleFieldText;
            set => Set(() => TitleFieldText, ref titleFieldText, value);
        }

        private string seriesNumberFieldText;

        public string SeriesNumberFieldText
        {
            get => seriesNumberFieldText;
            set => Set(() => SeriesNumberFieldText, ref seriesNumberFieldText, value);
        }

        private bool isRead;

        public bool IsReadCheck
        {
            get => isRead;
            set => Set(() => IsReadCheck, ref isRead, value);
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

        public ICommand RemoveAuthorCommand { get => new RelayCommand<int>(this.HandleRemoveAuthor); }

        private void HandleRemoveAuthor(int id)
        {
            var obsAuthor = AuthorsCollection.Where(c => c.Id == id).FirstOrDefault();
            Author toRemove = obsAuthor.Author;
            AuthorsCollection.Remove(obsAuthor);

            Task.Run(() =>
            {
                using ElibContext database = ApplicationSettings.CreateContext();
                database.Authors.Attach(toRemove);
                if (database.Books.Where(b => b.Authors.Where(c => c.Id == toRemove.Id).Any()).Count() <= 1)
                {
                    database.Entry(toRemove).State = EntityState.Deleted;
                    database.SaveChanges();
                }
            });
        }

        public ICommand RevertButtonCommand { get => new RelayCommand(this.HandleRevert); }

        private void HandleRevert()
        {
            this.ClearErrors();
            AuthorsCollection = new ObservableCollection<ObservableAuthor>(Book.Authors);
            Series = Book.Series == null ? null : new ObservableSeries(new BookSeries { Name = Book.Series.Name, Id = Book.Series.Id });
            TitleFieldText = Book.Title;
            SeriesNumberFieldText = Book.NumberInSeries.ToString();
            IsFavoriteCheck = Book.IsFavorite;
            IsReadCheck = Book.IsRead;
            Cover = Book.Cover;
        }

        public ICommand SaveButtonCommand { get => new RelayCommand(this.HandleSave); }

        private void HandleSave()
        {
            this.Validate();
            if (!this.HasErrors)
            {
                Task.Run(() =>
                {
                    var book = Book;
                    using ElibContext database = ApplicationSettings.CreateContext();

                    database.Books.Attach(Book.Book);

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

                    book.Title = TitleFieldText;
                    book.IsFavorite = IsFavoriteCheck;
                    book.IsRead = IsReadCheck;
                    book.Authors.Clear();
                    foreach (var author in AuthorsCollection)
                        book.Authors.Add(author);
                    book.Cover = Cover;

                    database.SaveChanges();
                });

                MessengerInstance.Send(new ShowBookDetailsMessage(Book));
            }
        }

        public ICommand CancelButtonCommand { get => new RelayCommand(this.HandleCancel); }

        private void HandleCancel()
        {
            MessengerInstance.Send(new ShowBookDetailsMessage(Book));
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
    }
}