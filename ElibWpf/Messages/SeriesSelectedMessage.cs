using Domain;

using GalaSoft.MvvmLight.Messaging;

namespace ElibWpf.Messages
{
    public class SeriesSelectedMessage : MessageBase
    {
        public SeriesSelectedMessage(BookSeries series)
        {
            this.Series = series;
        }

        public BookSeries Series { get; }
    }
}