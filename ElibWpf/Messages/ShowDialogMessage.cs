namespace ElibWpf.Messages
{
    public class ShowDialogMessage
    {
        public string Title { get; }

        public string Text { get; }

        public ShowDialogMessage(string title, string text)
        {
            Title = title;
            Text = text;
        }
    }
}