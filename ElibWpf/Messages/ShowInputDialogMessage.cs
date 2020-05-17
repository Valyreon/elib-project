using System;

namespace ElibWpf.Messages
{
    public class ShowInputDialogMessage
    {
        public ShowInputDialogMessage(string title, string text, Action<string> func)
        {
            this.Title = title;
            this.Text = text;
            this.CallOnResult = func;
        }

        public Action<string> CallOnResult { get; }

        public string Text { get; }

        public string Title { get; }
    }
}