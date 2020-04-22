using Domain;

using System;

namespace ElibWpf.BindingItems
{
    public class PaneMainItem
    {
        public PaneMainItem(string paneCaption, string faIconName, string viewerCaption, Func<Book, bool> condition, bool isSelectedBooksPane = false)
        {
            this.Condition = condition;
            this.IsSelectedBooksPane = isSelectedBooksPane;
            this.PaneCaption = paneCaption;
            this.FaIconName = faIconName;
            this.ViewerCaption = viewerCaption;
        }

        public Func<Book, bool> Condition { get; }
        public bool IsSelectedBooksPane { get; }
        public string FaIconName { get; }
        public string PaneCaption { get; }
        public string ViewerCaption { get; }
    }
}