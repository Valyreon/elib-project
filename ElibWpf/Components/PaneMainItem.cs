using Domain;
using System;

namespace ElibWpf.Components
{
    public class PaneMainItem
    {
        public string PaneCaption { get; }
        public string ViewerCaption { get; }
        public string FaIconName { get; }
        public Func<Book, bool> BooksQuery { get; }

        public PaneMainItem(Func<Book, bool> booksQuery, string viewCaption, string paneCaption, string faIconName)
        {
            this.BooksQuery = booksQuery;
            this.ViewerCaption = viewCaption;
            this.PaneCaption = paneCaption;
            this.FaIconName = faIconName;
        }
    }
}
