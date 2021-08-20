namespace Valyreon.Elib.EBookTools
{
    public abstract class EbookParser
    {
        public StyleSettings StyleSettings { get; set; }

        public abstract ParsedBook Parse();

        public abstract string GenerateHtml();

        /// <summary>
        ///     Generates <head> tag for Html book.
        /// </summary>
        /// <returns>String containg header node.</returns>
        protected string GenerateHeader(string title = null)
        {
            var header =
                "<head>\n" +
                "<meta charset=\"utf-8\">\n" +
                (title == null ? "" : "<title>" + title + "</title>") +
                "<style>\n" + StyleSettings.GenerateCss() +
                "</style>\n" +
                "<script>\n" +
                "</script>\n" +
                "</head>\n";

            return header;
        }
    }
}
