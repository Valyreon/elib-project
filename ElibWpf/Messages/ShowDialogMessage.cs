namespace ElibWpf.Messages
{
	public class ShowDialogMessage
	{
		public ShowDialogMessage(string title, string text)
		{
			Title = title;
			Text = text;
		}

		public string Text { get; }

		public string Title { get; }
	}
}
