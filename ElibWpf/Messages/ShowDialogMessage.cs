using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
