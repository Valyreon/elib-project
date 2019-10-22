namespace EbookTools
{
    public abstract class EbookParser
    {
        public StyleSettings StyleSettings { get; set; }

        abstract public ParsedBook Parse();

        /// <summary>
        /// Generates <head> tag for Html book.
        /// </summary>
        /// <returns>String containg header node.</returns>
        protected string GenerateHeader(string title = null)
        {
            string header =
                "<head>\n" +
                "<meta charset=\"utf-8\">\n" +
                (title == null ? "" : ("<title>" + title + "</title>")) +
                "<link href=\"https://fonts.googleapis.com/css?family=Bitter:400,400i \" rel=\"stylesheet\">" +
                "<style>\n" +
                this.StyleSettings.GenerateCss() +
                "</style>\n" +
                "<script>\n" +
                "</script>\n" +
                "</head>\n";

            return header;
        }
    }
}