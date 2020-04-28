using Domain;
using Models.Observables;

namespace ElibWpf.Messages
{
    public class CollectionSelectedMessage
    {
        public ObservableUserCollection Collection { get; }

        public CollectionSelectedMessage(ObservableUserCollection collection)
        {
            this.Collection = collection;
        }
    }
}