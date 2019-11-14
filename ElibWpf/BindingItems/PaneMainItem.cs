using Domain;

using System;

namespace ElibWpf.BindingItems
{
    public class PaneMainItem
    {
        public PaneMainItem(string paneCaption, string faIconName, string viewerCaption, Func<Book, bool> condition)
        {
            this.Condition = condition;
            this.PaneCaption = paneCaption;
            this.FaIconName = faIconName;
            this.ViewerCaption = viewerCaption;
        }

        public Func<Book, bool> Condition { get; }
        public string FaIconName { get; }
        public string PaneCaption { get; }
        public string ViewerCaption { get; }
    }
}