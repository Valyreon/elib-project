using MVVMLibrary;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace Domain
{
    [Table("Books")]
    public class Book : ObservableObject
    {
        private string title;
        private bool isFavorite;
        private bool isRead;
        private bool isMarked;
        private decimal? numberInSeries;
        private Cover cover;

        [Required]
        public EFile File { get; set; }

        public int FileId { get; set; }
        public int? CoverId { get; set; }
        public int Id { get; set; }

        public decimal PercentageRead { get; set; } = 0;

        private BookSeries series;

        public BookSeries Series
        {
            get => series;
            set
            {
                series = value;
                RaisePropertyChanged(() => Series);
                RaisePropertyChanged(() => SeriesInfo);
            }
        }

        public int? SeriesId { get; set; }

        public ObservableCollection<UserCollection> collections;

        public ObservableCollection<UserCollection> Collections
        {
            get => collections;
            set
            {
                collections = value;
                RaisePropertyChanged(() => Collections);
            }
        }

        public ObservableCollection<Quote> Quotes { get; set; }

        public ObservableCollection<Author> authors;

        public ObservableCollection<Author> Authors
        {
            get => authors;
            set
            {
                authors = value;
                RaisePropertyChanged(() => Authors);
                RaisePropertyChanged(() => AuthorsInfo);
            }
        }

        public int? WhenRead { get; set; }

        [Required]
        public string Title
        {
            get => this.title;
            set => Set(() => Title, ref title, value);
        }

        public bool IsFavorite
        {
            get => this.isFavorite;
            set => Set(() => IsFavorite, ref isFavorite, value);
        }

        public bool IsRead
        {
            get => this.isRead;
            set => Set(() => IsRead, ref isRead, value);
        }

        public bool IsMarked
        {
            get => this.isMarked;
            set => Set(() => IsMarked, ref isMarked, value);
        }

        public decimal? NumberInSeries
        {
            get => this.numberInSeries;
            set => Set(() => NumberInSeries, ref numberInSeries, value);
        }

        public Cover Cover
        {
            get => this.cover;
            set => Set(() => Cover, ref cover, value);
        }

        public string AuthorsInfo
        {
            get { return this.Authors.Any() ? this.Authors.Select(a => a.Name).Aggregate((i, j) => i + ", " + j) : ""; }
        }

        public string SeriesInfo =>
            this.Series != null
                ? $"{this.Series.Name} {(this.NumberInSeries != null ? $"#{this.NumberInSeries}" : "")}"
                : "";
    }
}