using DataLayer;
using System;

namespace ElibWpf.BindingItems
{
    public class PaneMainItem
    {
        public PaneMainItem(string paneCaption, Enum faIconName, string viewerCaption, FilterParameters filter)
        {
            this.Filter = filter;
            this.PaneCaption = paneCaption;
            this.Icon = faIconName;
            this.ViewerCaption = viewerCaption;
        }

        public FilterParameters Filter { get; }
        public Enum Icon { get; }
        public string PaneCaption { get; }
        public string ViewerCaption { get; }
    }
}