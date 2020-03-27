using Domain;
using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElibWpf.ViewModels.Flyouts
{
    public class EditBookViewModel : ViewModelBase
    {
        public Book Book { get; private set; }

        public EditBookViewModel(Book book)
        {
            this.Book = book;
            UserCollections = new ObservableCollection<UserCollection>(Book.UserCollections);
            AuthorsCollection = new ObservableCollection<Author>(Book.Authors);
            SeriesFieldText = Book.Series?.Name;
            TitleFieldText = Book.Title;
            SeriesNumberFieldText = Book.NumberInSeries.ToString();
            App.Database.Entry(Book).Collection(f => f.Files).Load();
            FilesCollection = new ObservableCollection<EFile>(Book.Files);
        }

        public ObservableCollection<UserCollection> UserCollections { get; }

        public ObservableCollection<Author> AuthorsCollection { get; }

        public ObservableCollection<EFile> FilesCollection { get; }

        private string seriesFieldText;
        public string SeriesFieldText
        {
            get => seriesFieldText;
            set => Set(() => SeriesFieldText, ref seriesFieldText, value);
        }

        private string titleFieldText;
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
    }
}
