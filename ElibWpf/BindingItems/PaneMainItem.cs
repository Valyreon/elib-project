using System;
using DataLayer;
using Models.Options;

namespace ElibWpf.BindingItems
{
    public class PaneMainItem
    {
        public PaneMainItem(string paneCaption, Enum faIconName, string viewerCaption, FilterAlt filter)
        {
            this.Filter = filter;
            this.PaneCaption = paneCaption;
            this.Icon = faIconName;
            this.ViewerCaption = viewerCaption;
        }

        public FilterAlt Filter { get; }
        public Enum Icon { get; }
        public string PaneCaption { get; }
        public string ViewerCaption { get; }
    }
}