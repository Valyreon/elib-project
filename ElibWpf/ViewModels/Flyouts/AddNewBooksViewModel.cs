using Domain;
using GalaSoft.MvvmLight;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;

namespace ElibWpf.ViewModels.Flyouts
{
    public class AddNewBooksViewModel : ViewModelBase
    {
        private IList<Book> books;

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

        private byte[] coverImage;
        public byte[] Cover
        {
            get => coverImage;
            set => Set(() => Cover, ref coverImage, value);
        }
    }
}
