using Domain;

namespace ElibWpf.Messages
{
    public class CollectionSelectedMessage
    {
        public UserCollection Collection { get; }

        public CollectionSelectedMessage(UserCollection collection)
        {
            this.Collection = collection;
        }
    }
}