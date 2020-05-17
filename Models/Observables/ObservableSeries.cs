using System;
using Domain;
using GalaSoft.MvvmLight;

namespace Models.Observables
{
    public class ObservableSeries : ObservableObject
    {
        public ObservableSeries(BookSeries series)
        {
            if (series is null)
            {
                throw new ArgumentException("Constructor argument can't be null.");
            }

            this.Series = series;
        }

        public int Id => this.Series.Id;

        public string Name
        {
            get => this.Series.Name;
            set
            {
                this.Series.Name = value;
                this.RaisePropertyChanged(() => this.Name);
            }
        }

        public BookSeries Series { get; }
    }
}