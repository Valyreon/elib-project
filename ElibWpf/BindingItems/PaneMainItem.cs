using Domain;
using ElibWpf.ViewModels.Controls;
using System;

namespace ElibWpf.BindingItems
{
    public class PaneMainItem
    {
        public string PaneCaption { get; }
        public string FaIconName { get; }
        public Func<Book, bool> Condition { get; }
        public string ViewerCaption { get; }

        public PaneMainItem(string paneCaption, string faIconName, string viewerCaption, Func<Book, bool> condition)
        {
            this.Condition = condition;
            this.PaneCaption = paneCaption;
            this.FaIconName = faIconName;
            this.ViewerCaption = viewerCaption;
        }
    }
}
