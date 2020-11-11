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
			get => title;
			set => Set(() => Title, ref title, value);
		}

		public bool IsFavorite
		{
			get => isFavorite;
			set => Set(() => IsFavorite, ref isFavorite, value);
		}

		public bool IsRead
		{
			get => isRead;
			set => Set(() => IsRead, ref isRead, value);
		}

		public bool IsMarked
		{
			get => isMarked;
			set => Set(() => IsMarked, ref isMarked, value);
		}

		public decimal? NumberInSeries
		{
			get => numberInSeries;
			set => Set(() => NumberInSeries, ref numberInSeries, value);
		}

		public Cover Cover
		{
			get => cover;
			set => Set(() => Cover, ref cover, value);
		}

		public string AuthorsInfo => Authors.Any() ? Authors.Select(a => a.Name).Aggregate((i, j) => i + ", " + j) : "";

		public string SeriesInfo =>
			Series != null
				? $"{Series.Name} {(NumberInSeries != null ? $"#{NumberInSeries}" : "")}"
				: "";
	}
}
