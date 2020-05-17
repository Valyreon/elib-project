using System;
using Domain;

namespace ElibWpf.BindingItems
{
    public class PaneMainItem
    {
        public PaneMainItem(string paneCaption, Enum faIconName, string viewerCaption, Func<Book, bool> condition,
            bool isSelectedBooksPane = false)
        {
            this.Condition = condition;
            this.IsSelectedBooksPane = isSelectedBooksPane;
            this.PaneCaption = paneCaption;
            this.Icon = faIconName;
            this.ViewerCaption = viewerCaption;
        }

        public Func<Book, bool> Condition { get; }
        public Enum Icon { get; }
        public bool IsSelectedBooksPane { get; }
        public string PaneCaption { get; }
        public string ViewerCaption { get; }
    }
}