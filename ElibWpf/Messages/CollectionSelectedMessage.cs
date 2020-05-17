using Models.Observables;

namespace ElibWpf.Messages
{
    public class CollectionSelectedMessage
    {
        public CollectionSelectedMessage(ObservableUserCollection collection)
        {
            this.Collection = collection;
        }

        public ObservableUserCollection Collection { get; }
    }
}