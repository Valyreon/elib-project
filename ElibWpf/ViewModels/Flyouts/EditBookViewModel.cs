﻿using System.Collections.ObjectModel;
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

                return (Brush) new BrushConverter().ConvertFrom("#bbb");
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
            if (this.AuthorsCollection.Any(c => c.Name == name)) // check if book is already in that collection
            {
                this.MessengerInstance.Send(new ShowDialogMessage("", "This author is already added."));
            }
            else // if not
            {
                Author newAuthor = new Author {Name = name};
                this.AuthorsCollection.Add(newAuthor);

                _ = Task.Run(() =>
                {
                    using var uow = ApplicationSettings.CreateUnitOfWork();
                    uow.AuthorRepository.AddAuthorForBook(newAuthor, this.Book.Id);
                });
            }
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
            });
        }

        private void HandleRevert()
        {
            this.ClearErrors();
            this.AuthorsCollection = new ObservableCollection<Author>(this.Book.Authors);
            this.Series = this.Book.Series == null
                ? null
                : new BookSeries {Name = this.Book.Series.Name, Id = this.Book.Series.Id};
            this.TitleFieldText = this.Book.Title;
            this.SeriesNumberFieldText = this.Book.NumberInSeries.ToString();
            this.IsFavoriteCheck = this.Book.IsFavorite;
            this.IsReadCheck = this.Book.IsRead;
            this.Cover = this.Book.Cover;
        }

        private void HandleSave()
        {
            this.Validate();
            if (this.HasErrors)
            {
                return;
            }

            Task.Run(() =>
            {
                Book book = this.Book;
                using var uow = ApplicationSettings.CreateUnitOfWork();

                if ((this.Book.Series == null && this.Series != null) || (this.Series != null && this.Book.Series.Id != this.Series.Id))
                {
                    this.Book.Series = uow.SeriesRepository.Find(this.Series.Id);
                }
                else if (this.Book.Series != null && this.Series != null && this.Book.Series.Id != this.Series.Id)
                {
                    this.Book.Series.Name = this.Series.Name;
                    uow.SeriesRepository.Update(this.Book.Series);
                }
                else
                {
                    this.Book.Series = null;
                }

                if (this.IsSeriesSelected)
                {
                    if (Regex.IsMatch(this.SeriesNumberFieldText, @"\d+(\.\d+)?"))
                    {
                        this.Book.NumberInSeries = decimal.Parse(this.SeriesNumberFieldText);
                    }
                }

                book.Title = this.TitleFieldText;
                book.IsFavorite = this.IsFavoriteCheck;
                book.IsRead = this.IsReadCheck;
                book.Authors.Clear();
                foreach (Author author in this.AuthorsCollection)
                {
                    book.Authors.Add(author);
                }

                book.Cover = this.Cover;
                uow.BookRepository.Update(this.Book);
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
            BookSeries newSeries = new BookSeries {Name = name};
            this.Series = newSeries;

            _ = Task.Run(() =>
            {
                using var uow = ApplicationSettings.CreateUnitOfWork();
                uow.SeriesRepository.Add(newSeries);
            });
        }

        private void HandleClearSeries()
        {
            this.Series = null;
        }

        private async void HandleEditSeries()
        {
            this.Series.Name = await DialogCoordinator.Instance.ShowInputAsync(this, "Edit Series", "Series name:",
                new MetroDialogSettings {DefaultText = this.Series.Name});
        }
    }
}