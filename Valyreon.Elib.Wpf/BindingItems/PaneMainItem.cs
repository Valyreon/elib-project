using Valyreon.Elib.DataLayer.Filters;

namespace Valyreon.Elib.Wpf.BindingItems
{
    public class PaneMainItem
    {
        public PaneMainItem(string paneCaption, string viewerCaption, BookFilter filter)
        {
            Filter = filter;
            PaneCaption = paneCaption;
            ViewerCaption = viewerCaption;
        }

        public BookFilter Filter { get; }
        public string PaneCaption { get; }
        public string ViewerCaption { get; }
    }
}
