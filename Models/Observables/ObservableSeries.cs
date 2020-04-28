using Domain;
using GalaSoft.MvvmLight;
using System;

namespace Models.Observables
{
    public class ObservableSeries : ObservableObject
    {
        private readonly BookSeries series;

        public BookSeries Series { get => series; }

        public ObservableSeries(BookSeries series)
        {
            if (series is null)
                throw new ArgumentException("Constructor argument can't be null.");
            this.series = series;
        }

        public string Name
        {
            get => series.Name;
            set
            {
                this.series.Name = value;
                RaisePropertyChanged(() => Name);
            }
        }

        public int Id { get => series.Id; }
    }
}
