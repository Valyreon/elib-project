using Domain;
using ElibWpf.Messages;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ElibWpf.ViewModels.Flyouts
{
    public class EditBookViewModel : ViewModelWithValidation
    {
        public Book Book { get; private set; }

        public EditBookViewModel(Book book)
        {
            this.Book = book;
            HandleRevert();
        }

        public ObservableCollection<Author> AuthorsCollection { get; private set; }

        public ObservableCollection<EFile> FilesCollection { get; private set; }

        private string seriesFieldText;
        public string SeriesFieldText
        {
            get => seriesFieldText;
            set => Set(() => SeriesFieldText, ref seriesFieldText, value);
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

        private bool isFavorite;
        public bool IsFavoriteCheck
        {
            get => isFavorite;
            set => Set(() => IsFavoriteCheck, ref isFavorite, value);
        }

        public ICommand AddAuthorCommand { get => new RelayCommand<string>(this.HandleAddAuthor); }

        private async void HandleAddAuthor(string name)
        {
            if (!string.IsNullOrWhiteSpace(name))
            {
                name = name.Trim();
                if (AuthorsCollection.Where(c => c.Name == name).Any()) // check if book is already in that collection
                {
                    MessengerInstance.Send(new ShowDialogMessage("", $"This book is already in the '{name}' collection"));
                }
                else // if not
                {
                    var existingAuthor = await Task.Run(() => App.Database.Authors.Where(c => c.Name == name).FirstOrDefault());
                    if (existingAuthor == null)
                    {
                        Author newAuthor = new Author
                        {
                            Name = name
                        };
                        AuthorsCollection.Add(newAuthor);
                    }
                    else
                    {
                        AuthorsCollection.Add(existingAuthor);
                    }
                }
            }
            AddAuthorFieldText = "";
        }

        public ICommand RemoveAuthorCommand { get => new RelayCommand<int>((x) => AuthorsCollection.Remove(AuthorsCollection.Where(c => c.Id == x).FirstOrDefault())); }

        public ICommand RevertButtonCommand { get => new RelayCommand(this.HandleRevert); }

        private void HandleRevert()
        {
            this.ClearErrors();
            AuthorsCollection = new ObservableCollection<Author>(Book.Authors);
            SeriesFieldText = Book.Series?.Name;
            TitleFieldText = Book.Title;
            SeriesNumberFieldText = Book.NumberInSeries.ToString();
            App.Database.Entry(Book).Collection(f => f.Files).Load();
            FilesCollection = new ObservableCollection<EFile>(Book.Files);
            IsFavoriteCheck = Book.IsFavorite;
            IsReadCheck = Book.IsRead;
        }

        public ICommand SaveButtonCommand { get => new RelayCommand(this.HandleSave); }

        private async void HandleSave()
        {
            this.Validate();
            if (!this.HasErrors)
            {
                Book.Title = TitleFieldText;
                if (!string.IsNullOrWhiteSpace(SeriesFieldText))
                {
                    if (Book.Series == null)
                    {
                        Book.Series = new BookSeries();
                    }
                    Book.Series.Name = SeriesFieldText;
                    Book.NumberInSeries = decimal.Parse(SeriesNumberFieldText);
                }
                Book.IsFavorite = IsFavoriteCheck;
                Book.IsRead = IsReadCheck;
                Book.Authors = new List<Author>(AuthorsCollection);
                Book.Files = new List<EFile>(FilesCollection);
                await App.Database.SaveChangesAsync();
                MessengerInstance.Send(new ShowBookDetailsMessage(Book));
            }
        }

        public ICommand CancelButtonCommand { get => new RelayCommand(this.HandleCancel); }

        private void HandleCancel()
        {
            MessengerInstance.Send(new ShowBookDetailsMessage(Book));
        }


    }
}
