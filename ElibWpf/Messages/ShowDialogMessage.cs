namespace ElibWpf.Messages
{
    public class ShowDialogMessage
    {
        public ShowDialogMessage(string title, string text)
        {
            this.Title = title;
            this.Text = text;
        }

        public string Text { get; }

        public string Title { get; }
    }
}