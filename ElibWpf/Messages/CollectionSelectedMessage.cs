namespace ElibWpf.Messages
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
