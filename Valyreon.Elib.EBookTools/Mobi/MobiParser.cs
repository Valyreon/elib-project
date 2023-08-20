using System.IO;
using System.Text;
using HtmlAgilityPack;
using Valyreon.Elib.EBookTools;

namespace Valyreon.Elib.EbookTools.Mobi
{
    public class MobiParser : EbookParser
    {
        private readonly string filePath;

        public MobiParser(string filePath, StyleSettings settings)
        {
            StyleSettings = settings;
            this.filePath = filePath;
        }

        public MobiParser(string filePath)
        {
            StyleSettings = StyleSettings.Default;
            this.filePath = filePath;
        }

        /// <summary>
        ///     Parses the mobi file and creates ParsedBook with generated html.
        /// </summary>
        public override ParsedBook Parse()
        {
            var mf = MobiFile.LoadFile(File.ReadAllBytes(filePath));

            return new ParsedBook
            {
                Title = mf.Name.Clean(),
                Authors = new System.Collections.Generic.List<string> { mf.Creator.Clean() },
                Path = filePath
            };
        }

        public override string GenerateHtml()
        {
            var build = new StringBuilder();
            var mf = MobiFile.LoadFile(File.ReadAllBytes(filePath));
            build.Append(GenerateHeader());
            build.Append("<body>\n");
            var html = mf.BookText;
            var doc = new HtmlDocument();
            doc.LoadHtml(html);
            var bodyContent = doc.DocumentNode.SelectSingleNode("//body"); // get the <body> node

            build.Append(bodyContent.InnerHtml);
            build.Append("</body>");
            return build.ToString();
        }
    }
}
