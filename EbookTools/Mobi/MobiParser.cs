using HtmlAgilityPack;

using System.IO;
using System.Text;

namespace EbookTools.Mobi
{
    public class MobiParser : EbookParser
    {
        private readonly byte[] _rawFile;

        public MobiParser(byte[] file, StyleSettings settings)
        {
            this.StyleSettings = settings;
            this._rawFile = file;
        }

        public MobiParser(byte[] file)
        {
            this.StyleSettings = StyleSettings.Default;
            this._rawFile = file;
        }

        public MobiParser(Stream file, StyleSettings settings)
        {
            this.StyleSettings = settings;
            MemoryStream ms = new MemoryStream();
            file.CopyTo(ms);
            this._rawFile = ms.GetBuffer();
        }

        public MobiParser(Stream file)
        {
            this.StyleSettings = StyleSettings.Default;
            MemoryStream ms = new MemoryStream();
            file.CopyTo(ms);
            this._rawFile = ms.GetBuffer();
        }

        /// <summary>
        /// Parses the mobi file and creates ParsedBook with generated html.
        /// </summary>
        public override ParsedBook Parse()
        {
            MobiFile mf = MobiFile.LoadFile(this._rawFile);
            string html = mf.BookText;
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(html);
            return new ParsedBook(mf.Name, null, null, null, null, ".mobi", _rawFile);
        }

        public override string GenerateHtml()
        {
            StringBuilder build = new StringBuilder();
            MobiFile mf = MobiFile.LoadFile(this._rawFile);
            build.Append(GenerateHeader());
            build.Append("<body>\n");
            string html = mf.BookText;
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(html);
            HtmlNode bodyContent = doc.DocumentNode.SelectSingleNode("//body"); // get the <body> node

            build.Append(bodyContent.InnerHtml);
            build.Append("</body>");
            return build.ToString();
        }
    }
}