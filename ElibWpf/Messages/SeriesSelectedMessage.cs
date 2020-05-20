using GalaSoft.MvvmLight.Messaging;
using Models.Observables;

namespace ElibWpf.Messages
{
    public class SeriesSelectedMessage : MessageBase
    {
        public SeriesSelectedMessage(int seriesId)
        {
            this.SeriesId = seriesId;
        }

        public int SeriesId { get; }
    }
}