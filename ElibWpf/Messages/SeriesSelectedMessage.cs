using Domain;
using GalaSoft.MvvmLight.Messaging;

namespace ElibWpf.Messages
{
    public class SeriesSelectedMessage: MessageBase
    {
        public BookSeries Series { get; }

        public SeriesSelectedMessage(BookSeries series)
        {
            this.Series = series;
        }
    }
}
