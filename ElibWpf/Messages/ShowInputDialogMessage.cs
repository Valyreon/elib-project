using System;

namespace ElibWpf.Messages
{
    public class ShowInputDialogMessage
    {
        public string Title { get; }

        public string Text { get; }

        public Action<string> CallOnResult { get; }

        public ShowInputDialogMessage(string title, string text, Action<string> func)
        {
            Title = title;
            Text = text;
            this.CallOnResult = func;
        }
    }
}