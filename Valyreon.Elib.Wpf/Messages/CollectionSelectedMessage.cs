namespace Valyreon.Elib.Wpf.Messages
{
    public class CollectionSelectedMessage
    {
        public CollectionSelectedMessage(int id)
        {
            CollectionId = id;
        }

        public int CollectionId { get; }
    }
}
