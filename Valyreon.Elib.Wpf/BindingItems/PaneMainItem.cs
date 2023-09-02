using Valyreon.Elib.DataLayer;

namespace Valyreon.Elib.Wpf.BindingItems
{
    public class PaneMainItem
    {
        public PaneMainItem(string paneCaption, string viewerCaption, FilterParameters filter)
        {
            Filter = filter;
            PaneCaption = paneCaption;
            ViewerCaption = viewerCaption;
        }

        public FilterParameters Filter { get; }
        public string PaneCaption { get; }
        public string ViewerCaption { get; }
    }
}
