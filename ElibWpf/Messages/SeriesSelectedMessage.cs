using MVVMLibrary.Messaging;

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