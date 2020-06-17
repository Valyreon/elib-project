

namespace ElibWpf.Messages
{
    public class CollectionSelectedMessage
    {
        public CollectionSelectedMessage(int id)
        {
            this.CollectionId = id;
        }

        public int CollectionId { get; }
    }
}