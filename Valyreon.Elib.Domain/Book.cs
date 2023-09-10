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
        private string title;
        private string description;
        private bool isFavorite;
        private bool isRead;
        private bool isMarked;
        private decimal? numberInSeries;
        private Cover cover;
        private ObservableCollection<Author> authors;
        private ObservableCollection<UserCollection> collections = new();
        private BookSeries series;
        private string path;
        private bool isFileMissing;
        private string isbn;

        [Column]
        public int? CoverId { get; set; }

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

        [Required]
        [Column]
        public string Title
        {
            get => title;
            set => Set(() => Title, ref title, value);
        }

        [Column]
        public string Description
        {
            get => description;
            set => Set(() => Description, ref description, value);
        }

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

        [Column]
        public bool IsRead
        {
            get => isRead;
            set => Set(() => IsRead, ref isRead, value);
        }

        [NotMapped]
        public bool IsMarked
        {
            get => isMarked;
            set => Set(() => IsMarked, ref isMarked, value);
        }

        [Column]
        public decimal? NumberInSeries
        {
            get => numberInSeries;
            set => Set(() => NumberInSeries, ref numberInSeries, value);
        }

        [ForeignKey("CoverId")]
        public Cover Cover
        {
            get => cover;
            set => Set(() => Cover, ref cover, value);
        }

        [Column]
        [Required]
        [StringLength(10)]
        public string Format { get; set; }

        [Column]
        [Required]
        [StringLength(64)]
        public string Signature { get; set; }

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

        [NotMapped]
        public string AuthorsInfo => Authors != null && Authors.Any() ? Authors.Select(a => a.Name).Aggregate((i, j) => i + ", " + j) : "";

        [NotMapped]
        public string SeriesInfo =>
            Series != null
                ? $"{Series.Name} {(NumberInSeries != null ? $"#{NumberInSeries}" : "")}"
                : "";

        [NotMapped]
        public bool IsFileMissing
        {
            get => isFileMissing;
            set => Set(() => IsFileMissing, ref isFileMissing, value);
        }

        [NotMapped]
        public bool IsLoaded { get; set; }
    }
}
