using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Linq;

namespace Valyreon.Elib.Domain
{
    [Table("Books")]
    public class Book : ObservableEntity
    {
        private ObservableCollection<Author> authors;
        private ObservableCollection<UserCollection> collections = new();
        private Cover cover;
        private string description;
        private string isbn;
        private bool isFavorite;
        private bool isFileMissing;
        private bool isMarked;
        private bool isRead;
        private decimal? numberInSeries;
        private string path;
        private BookSeries series;
        private string title;

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

        [NotMapped]
        public string AuthorsInfo => Authors != null && Authors.Any() ? Authors.Select(a => a.Name).Aggregate((i, j) => i + ", " + j) : "";

        public ObservableCollection<UserCollection> Collections
        {
            get => collections;
            set
            {
                collections = value;
                RaisePropertyChanged(() => Collections);
            }
        }

        [ForeignKey("CoverId")]
        public Cover Cover
        {
            get => cover;
            set => Set(() => Cover, ref cover, value);
        }

        [Column]
        public int? CoverId { get; set; }

        [Column]
        public string Description
        {
            get => description;
            set => Set(() => Description, ref description, value);
        }

        [Column]
        [Required]
        [StringLength(10)]
        public string Format { get; set; }

        [Column]
        public string ISBN
        {
            get => isbn;
            set => Set(() => ISBN, ref isbn, value);
        }

        [Column]
        public bool IsFavorite
        {
            get => isFavorite;
            set => Set(() => IsFavorite, ref isFavorite, value);
        }

        [NotMapped]
        public bool IsFileMissing
        {
            get => isFileMissing;
            set => Set(() => IsFileMissing, ref isFileMissing, value);
        }

        [NotMapped]
        public bool IsLoaded { get; set; }

        [NotMapped]
        public bool IsMarked
        {
            get => isMarked;
            set => Set(() => IsMarked, ref isMarked, value);
        }

        [Column]
        public bool IsRead
        {
            get => isRead;
            set => Set(() => IsRead, ref isRead, value);
        }

        [Column]
        public decimal? NumberInSeries
        {
            get => numberInSeries;
            set => Set(() => NumberInSeries, ref numberInSeries, value);
        }

        [Column]
        [Required]
        [StringLength(32767)]
        public string Path
        {
            get => path;
            set
            {
                Set(() => Path, ref path, value);
                IsFileMissing = !File.Exists(path);
            }
        }

        public ObservableCollection<Quote> Quotes { get; set; }

        [ForeignKey("SeriesId")]
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

        [Column]
        public int? SeriesId { get; set; }

        [NotMapped]
        public string SeriesInfo =>
            Series != null
                ? $"{Series.Name} {(NumberInSeries != null ? $"#{NumberInSeries}" : "")}"
                : "";

        [Column]
        [Required]
        [StringLength(64)]
        public string Signature { get; set; }

        [Required]
        [Column]
        public string Title
        {
            get => title;
            set => Set(() => Title, ref title, value);
        }
    }
}
