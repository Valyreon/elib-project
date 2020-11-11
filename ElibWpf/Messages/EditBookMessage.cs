using Domain;

namespace ElibWpf.Messages
{
	public class EditBookMessage
	{
		public EditBookMessage(Book clickedBook)
		{
			Book = clickedBook;
		}

		public Book Book { get; }
	}
}
