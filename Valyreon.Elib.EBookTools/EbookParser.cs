namespace Valyreon.Elib.EBookTools
{
    public abstract class EbookParser
    {
        public StyleSettings StyleSettings { get; set; }

        public abstract string GenerateHtml();

        public abstract ParsedBook Parse();

        /// <summary>
        ///     Generates <head> tag for Html book.
        /// </summary>
        /// <param name="title">Title of the book.</param>
        /// <returns>String containg header node.</returns>
        protected string GenerateHeader(string title = null)
        {
            return "<head>\n" +
                "<meta charset=\"utf-8\">\n" +
                (title == null ? "" : "<title>" + title + "</title>") +
                "<style>\n" + StyleSettings.GenerateCss() +
                "</style>\n" +
                "<script>\n" +
                "</script>\n" +
                "</head>\n";
        }
    }
}
