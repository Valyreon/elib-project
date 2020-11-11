using HtmlAgilityPack;
using System.IO;
using System.Text;

namespace EbookTools.Mobi
{
	public class MobiParser : EbookParser
	{
		private readonly byte[] rawFile;

		public MobiParser(byte[] file, StyleSettings settings)
		{
			StyleSettings = settings;
			rawFile = file;
		}

		public MobiParser(byte[] file)
		{
			StyleSettings = StyleSettings.Default;
			rawFile = file;
		}

		public MobiParser(Stream file, StyleSettings settings)
		{
			StyleSettings = settings;
			using var ms = new MemoryStream();
			file.CopyTo(ms);
			rawFile = ms.GetBuffer();
		}

		public MobiParser(Stream file)
		{
			StyleSettings = StyleSettings.Default;
			using var ms = new MemoryStream();
			file.CopyTo(ms);
			rawFile = ms.GetBuffer();
		}

		/// <summary>
		///     Parses the mobi file and creates ParsedBook with generated html.
		/// </summary>
		public override ParsedBook Parse()
		{
			var mf = MobiFile.LoadFile(rawFile);
			var html = mf.BookText;
			var doc = new HtmlDocument();
			doc.LoadHtml(html);
			return new ParsedBook(mf.Name, null, null, null, null, ".mobi", rawFile);
		}

		public override string GenerateHtml()
		{
			var build = new StringBuilder();
			var mf = MobiFile.LoadFile(rawFile);
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
