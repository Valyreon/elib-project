using GalaSoft.MvvmLight.Messaging;
using Models.Observables;

namespace ElibWpf.Messages
{
    public class SeriesSelectedMessage : MessageBase
    {
        public SeriesSelectedMessage(ObservableSeries series)
        {
            this.Series = series;
        }

        public ObservableSeries Series { get; }
    }
}