using Valyreon.Elib.Domain;
using Valyreon.Elib.Mvvm.Messaging;

namespace Valyreon.Elib.Wpf.Messages
{
    public class SeriesSelectedMessage : MessageBase
    {
        public SeriesSelectedMessage(BookSeries series)
        {
            Series = series;
        }

        public BookSeries Series { get; }
    }
}
