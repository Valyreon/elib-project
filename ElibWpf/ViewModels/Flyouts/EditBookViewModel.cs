using DataLayer;
using Domain;
using ElibWpf.Messages;
using ElibWpf.ValidationAttributes;
using GalaSoft.MvvmLight.Command;
using Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
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

        [NotEmpty(ErrorMessage = "Book has to have at least one author.")]
        public ObservableCollection<Author> AuthorsCollection { get; private set; }

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

        private byte[] coverImage;

        public byte[] Cover
        {
            get => coverImage;
            set => Set(() => Cover, ref coverImage, value);
        }

        public ICommand AddAuthorButtonCommand { get => new RelayCommand(() => { MessengerInstance.Send(new ShowInputDialogMessage("Adding New Author", "Author's name:", this.HandleAddAuthor)); }); }

        private void HandleAddAuthor(string name)
        {
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
                    AuthorsCollection.Add(newAuthor);

                    Task.Run(() =>
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
            AddAuthorFieldText = "";
        }

        public ICommand RemoveAuthorCommand { get => new RelayCommand<int>((x) => AuthorsCollection.Remove(AuthorsCollection.Where(c => c.Id == x).FirstOrDefault())); }

        public ICommand RevertButtonCommand { get => new RelayCommand(this.HandleRevert); }

        private void HandleRevert()
        {
            using ElibContext database = ApplicationSettings.CreateContext();
            database.Books.Attach(Book);

            this.ClearErrors();
            AuthorsCollection = new ObservableCollection<Author>(Book.Authors);
            SeriesFieldText = Book.Series?.Name;
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
                    using ElibContext database = ApplicationSettings.CreateContext();
                    database.Books.Attach(Book);

                    Book.Title = TitleFieldText;
                    if (!string.IsNullOrWhiteSpace(SeriesFieldText))
                    {
                        if (Book.Series == null)
                        {
                            Book.Series = new BookSeries();
                        }
                        Book.Series.Name = SeriesFieldText;
                        if (Regex.IsMatch(SeriesNumberFieldText, @"\d+(\.\d+)?"))
                            Book.NumberInSeries = decimal.Parse(SeriesNumberFieldText);
                    }
                    Book.IsFavorite = IsFavoriteCheck;
                    Book.IsRead = IsReadCheck;
                    Book.Authors = new List<Author>(AuthorsCollection);
                    Book.Cover = Cover;

                    database.SaveChanges();
                });

                MessengerInstance.Send(new ShowBookDetailsMessage(Book));
                MessengerInstance.Send(new BookEditDoneMessage());
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

    }
}