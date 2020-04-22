using Domain;
using ElibWpf.Messages;
using ElibWpf.ValidationAttributes;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;

namespace ElibWpf.ViewModels.Flyouts
{
    public class AddNewBooksViewModel : ViewModelWithValidation
    {
        private IList<Book> books;
        private int counter = 0;

        private Book currentBook;
        private Book CurrentBook
        {
            get => currentBook;

            set
            {
                currentBook = value;
                Set(() => CurrentBook, ref currentBook, value);
                AuthorsCollection = new ObservableCollection<Author>(CurrentBook.Authors);
                SeriesFieldText = CurrentBook.Series?.Name;
                TitleFieldText = CurrentBook.Title;
                SeriesNumberFieldText = CurrentBook.NumberInSeries.ToString();
                FilesCollection = new ObservableCollection<EFile>(CurrentBook.Files);
                IsFavoriteCheck = CurrentBook.IsFavorite;
                IsReadCheck = CurrentBook.IsRead;
                Cover = CurrentBook.Cover;
            }
        }

        public AddNewBooksViewModel(IList<Book> newBooks)
        {
            books = newBooks;
            CurrentBook = books[0];
            TitleText = $"Book 1 of {books.Count}";
            ProceedButtonText = books.Count == 1 ? "SAVE & FINISH" :"SAVE & NEXT";
        }

        [NotEmpty(ErrorMessage = "Book has to have at least one author.")]
        public ObservableCollection<Author> AuthorsCollection { get; private set; }

        [NotEmpty(ErrorMessage = "Book has to have at least one file.")]
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

        private async void HandleAddAuthor(string name)
        {
            if (!string.IsNullOrWhiteSpace(name))
            {
                name = name.Trim();
                if (AuthorsCollection.Where(c => c.Name == name).Any()) // check if book already has that author
                {
                    MessengerInstance.Send(new ShowDialogMessage("", $"This author is already added."));
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
            AuthorsCollection = new ObservableCollection<Author>(CurrentBook.Authors);
            SeriesFieldText = CurrentBook.Series?.Name;
            TitleFieldText = CurrentBook.Title;
            SeriesNumberFieldText = CurrentBook.NumberInSeries.ToString();
            FilesCollection = new ObservableCollection<EFile>(CurrentBook.Files);
            IsFavoriteCheck = CurrentBook.IsFavorite;
            IsReadCheck = CurrentBook.IsRead;
            Cover = CurrentBook.Cover;
        }

        public ICommand NextButtonCommand { get => new RelayCommand(this.HandleSaveAndNext); }

        private async void HandleSaveAndNext()
        {
            this.Validate();
            if (!this.HasErrors)
            {
                CurrentBook.Title = TitleFieldText;
                if (!string.IsNullOrWhiteSpace(SeriesFieldText))
                {
                    if (CurrentBook.Series == null)
                    {
                        CurrentBook.Series = new BookSeries();
                    }
                    CurrentBook.Series.Name = SeriesFieldText;
                    if (Regex.IsMatch(SeriesNumberFieldText, @"\d+(\.\d+)?"))
                        CurrentBook.NumberInSeries = decimal.Parse(SeriesNumberFieldText);
                }
                CurrentBook.IsFavorite = IsFavoriteCheck;
                CurrentBook.IsRead = IsReadCheck;
                CurrentBook.Authors = new List<Author>(AuthorsCollection);
                CurrentBook.Files = new List<EFile>(FilesCollection);
                CurrentBook.Cover = Cover;
                App.Database.Books.Add(CurrentBook);
                await App.Database.SaveChangesAsync();

                TitleText = $"Book {counter+2} of {books.Count}";
                if (counter >= books.Count - 1)
                {
                    MessengerInstance.Send(new CloseFlyoutMessage());
                }
                else
                {
                    if(counter == books.Count - 2)
                    {
                        ProceedButtonText = "SAVE & FINISH";
                    }
                    this.ClearErrors();
                    CurrentBook = books[++counter];
                }
            }
        }

        public ICommand CancelButtonCommand { get => new RelayCommand(this.HandleCancel); }

        private void HandleCancel()
        {
            MessengerInstance.Send(new CloseFlyoutMessage());
        }

        public ICommand AddFileButtonCommand { get => new RelayCommand(this.HandleAddFileButton); }

        private void HandleAddFileButton()
        {
            using OpenFileDialog dlg = new OpenFileDialog
            {
                Filter = "Epub files|*.epub|Mobi files|*.mobi|All files|*.*",
                CheckFileExists = true,
                CheckPathExists = true,
                FilterIndex = 3,
                Multiselect = true
            };
            var result = dlg.ShowDialog();
            if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(dlg.FileName))
            {
                this.FilesCollection.Add(new EFile { Format = Path.GetExtension(dlg.FileName), RawContent = File.ReadAllBytes(dlg.FileName) });
            }
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
