using Domain;
using GalaSoft.MvvmLight;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

namespace Models.Observables
{
    public class ObservableBook : ObservableObject
    {
        private readonly Book book;
        private bool isSelected;

        public ObservableBook(Book book)
        {
            this.book = book;
            this.isSelected = false;

            this.Authors = new ObservableCollection<ObservableAuthor>(book.Authors.Select(x => new ObservableAuthor(x)));
            Authors.CollectionChanged += this.AuthorsCollectionChanged;

            this.Quotes = book.Quotes == null ? new ObservableCollection<Quote>() : new ObservableCollection<Quote>(book.Quotes);
            Quotes.CollectionChanged += this.QuotesCollectionChanged;

            this.Collections = book.UserCollections == null ? new ObservableCollection<ObservableUserCollection>() : new ObservableCollection<ObservableUserCollection>(book.UserCollections.Select(b => new ObservableUserCollection(b)));
            Quotes.CollectionChanged += this.CollectionsChanged;

            this.Series = (book.Series == null ? null : new ObservableSeries(book.Series));
            if(this.Series != null)
            {
                this.Series.PropertyChanged += (a, b) => this.RaisePropertyChanged(() => SeriesInfo);
            }
        }

        private void QuotesCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (var newItem in e.NewItems)
                {
                    book.Quotes.Add((Quote)newItem);
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (var oldItem in e.OldItems)
                {
                    book.Quotes.Add((Quote)oldItem);
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                book.Quotes.Clear();
            }
        }

        private void CollectionsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (var newItem in e.NewItems)
                {
                    book.UserCollections.Add(((ObservableUserCollection)newItem).Collection);
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (var oldItem in e.OldItems)
                {
                    book.UserCollections.Add(((ObservableUserCollection)oldItem).Collection);
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                book.Quotes.Clear();
            }
        }

        private void AuthorsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if(e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach(var newItem in e.NewItems)
                {
                    book.Authors.Add(((ObservableAuthor)newItem).Author);
                }
            }
            else if(e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (var oldItem in e.OldItems)
                {
                    book.Authors.Add(((ObservableAuthor)oldItem).Author);
                }
            }
            else if(e.Action == NotifyCollectionChangedAction.Reset)
            {
                book.Authors.Clear();
            }
            RaisePropertyChanged(() => AuthorsInfo);
        }

        public int Id { get => book.Id; }

        public ObservableCollection<ObservableAuthor> Authors { get; }
        public ObservableCollection<Quote> Quotes { get; }
        public ObservableCollection<ObservableUserCollection> Collections { get; }

        public ObservableSeries Series { get; set; }

        public bool IsSelected
        {
            get => isSelected;
            set => Set(() => IsSelected, ref isSelected, value);
        }

        public Book Book { get => book; }

        public string Title
        {
            get => book.Title;
            set
            {
                book.Title = value;
                RaisePropertyChanged(() => Title);
            }
        }

        public string SeriesInfo
        {
            get => book.Series != null ? $"{book.Series.Name} {((book.NumberInSeries != null) ? ($"#{book.NumberInSeries}") : (""))}" : "";
        }

        public decimal? NumberInSeries
        {
            get => book.NumberInSeries;
            set
            {
                book.NumberInSeries = value;
                RaisePropertyChanged(() => NumberInSeries);
            }
        }

        public byte[] Cover
        {
            get => book.Cover;
            set
            {
                book.Cover = value;
                RaisePropertyChanged(() => Cover);
            }
        }

        public bool IsRead
        {
            get => book.IsRead;
            set
            {
                book.IsRead = value;
                RaisePropertyChanged(() => IsRead);
            }
        }

        public bool IsFavorite
        {
            get => book.IsFavorite;
            set
            {
                book.IsFavorite = value;
                RaisePropertyChanged(() => IsFavorite);
            }
        }

        public string AuthorsInfo {
            get {
                if (Authors.Count() > 0)
                    return Authors.Select(a => a.Name).Aggregate((i, j) => i + ", " + j);
                else return "";
            }
        }
    }
}
