using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using Domain;
using GalaSoft.MvvmLight;

namespace Models.Observables
{
    public class ObservableBook : ObservableObject
    {
        private bool isSelected;

        private ObservableSeries series;

        public ObservableBook(Book book)
        {
            this.Book = book;

            this.Authors = new ObservableCollection<ObservableAuthor>(book.Authors.Select(x => new ObservableAuthor(x)));
            this.Authors.CollectionChanged += this.AuthorsCollectionChanged;

            this.Quotes = book.Quotes == null
                ? new ObservableCollection<Quote>()
                : new ObservableCollection<Quote>(book.Quotes);
            this.Quotes.CollectionChanged += this.QuotesCollectionChanged;

            this.Collections = book.UserCollections == null
                ? new ObservableCollection<ObservableUserCollection>()
                : new ObservableCollection<ObservableUserCollection>(
                    book.UserCollections.Select(b => new ObservableUserCollection(b)));
            this.Quotes.CollectionChanged += this.CollectionsChanged;

            this.Series = book.Series == null ? null : new ObservableSeries(book.Series);
            if (this.Series != null)
            {
                this.Series.PropertyChanged += (a, b) => this.RaisePropertyChanged(() => this.SeriesInfo);
            }
        }

        public ObservableCollection<ObservableAuthor> Authors { get; }

        public string AuthorsInfo
        {
            get { return this.Authors.Any() ? this.Authors.Select(a => a.Name).Aggregate((i, j) => i + ", " + j) : ""; }
        }

        public Book Book { get; }
        public ObservableCollection<ObservableUserCollection> Collections { get; }

        public byte[] Cover
        {
            get => this.Book.Cover;
            set
            {
                this.Book.Cover = value;
                this.RaisePropertyChanged(() => this.Cover);
            }
        }

        public int Id => this.Book.Id;

        public bool IsFavorite
        {
            get => this.Book.IsFavorite;
            set
            {
                this.Book.IsFavorite = value;
                this.RaisePropertyChanged(() => this.IsFavorite);
            }
        }

        public bool IsMarked
        {
            get => this.isSelected;
            set => this.Set(() => this.IsMarked, ref this.isSelected, value);
        }

        public bool IsRead
        {
            get => this.Book.IsRead;
            set
            {
                this.Book.IsRead = value;
                this.RaisePropertyChanged(() => this.IsRead);
            }
        }

        public decimal? NumberInSeries
        {
            get => this.Book.NumberInSeries;
            set
            {
                this.Book.NumberInSeries = value;
                this.RaisePropertyChanged(() => this.NumberInSeries);
            }
        }

        public ObservableCollection<Quote> Quotes { get; }

        public ObservableSeries Series
        {
            get => this.series;
            set
            {
                this.Set(() => this.Series, ref this.series, value);
                if (value != null)
                {
                    this.Book.Series = value.Series;
                    this.Book.SeriesId = value.Series.Id;
                }
                else
                {
                    this.Book.SeriesId = null;
                    this.Book.Series = null;
                    this.Book.NumberInSeries = null;
                }

                this.RaisePropertyChanged(() => this.SeriesInfo);
            }
        }

        public string SeriesInfo =>
            this.Book.Series != null
                ? $"{this.Book.Series.Name} {(this.Book.NumberInSeries != null ? $"#{this.Book.NumberInSeries}" : "")}"
                : "";

        public string Title
        {
            get => this.Book.Title;
            set
            {
                this.Book.Title = value;
                this.RaisePropertyChanged(() => this.Title);
            }
        }

        private void QuotesCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (object newItem in e.NewItems)
                {
                    this.Book.Quotes.Add((Quote) newItem);
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (object oldItem in e.OldItems)
                {
                    this.Book.Quotes.Add((Quote) oldItem);
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                this.Book.Quotes.Clear();
            }
        }

        private void CollectionsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (object newItem in e.NewItems)
                {
                    this.Book.UserCollections.Add(((ObservableUserCollection) newItem).Collection);
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (object oldItem in e.OldItems)
                {
                    this.Book.UserCollections.Add(((ObservableUserCollection) oldItem).Collection);
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                this.Book.Quotes.Clear();
            }
        }

        private void AuthorsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (object newItem in e.NewItems)
                {
                    this.Book.Authors.Add(((ObservableAuthor) newItem).Author);
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (object oldItem in e.OldItems)
                {
                    this.Book.Authors.Add(((ObservableAuthor) oldItem).Author);
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                this.Book.Authors.Clear();
            }

            this.RaisePropertyChanged(() => this.AuthorsInfo);
        }
    }
}