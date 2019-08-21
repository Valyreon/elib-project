using System.ComponentModel.DataAnnotations;

namespace EbookTools
{
	public class StyleSettings
	{
		public static readonly StyleSettings Default = new StyleSettings();

		[RegularExpression(@"#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})", ErrorMessage = "Invalid hex color.")]
		public string BackgroundColor { get; set; } = "#f2f1ef";

		[RegularExpression(@"#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})", ErrorMessage = "Invalid hex color.")]
		public string ForegroundColor { get; set; } = "#222222";

		public uint FontSize { get; set; } = 18;
		public string Font { get; set; } = "Bitter";

		[RegularExpression(@"[0-9]{1}\.[0-9]*", ErrorMessage = "Invalid line height.")]
		public string LineHeight { get; set; } = "1.6";

		public int SideMargins { get; set; } = 25;

		[RegularExpression(@"#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})", ErrorMessage = "Invalid hex color.")]
		public string LinkColor { get; set; } = "#4169e1";

		public StyleSettings(string bgColor, string fgColor, uint fontSize, string font)
		{
			this.BackgroundColor = bgColor;
			this.ForegroundColor = fgColor;
			this.FontSize = fontSize;
			this.Font = font;
		}

		public StyleSettings()
		{
		}

		/// <summary>
		/// Generates CSS based on current value of attributes.
		/// </summary>
		/// <returns>String containing css styling.</returns>
		public string GenerateCss()
		{
			string css =
				"\nbody {\n" +
				"	margin: 0 " + this.SideMargins + "%;\n" +
				"	background-color: " + this.BackgroundColor + ";\n" +
				"	color: " + this.ForegroundColor + ";\n" +
				"	font-family: " + "\"" + this.Font + "\"" + ", sans-serif;\n" +
				"	font-size: " + this.FontSize + "px;\n" +
				"	text-align: justify;\n" +
				"}\n" +
				"h1, h2, h3 {\n" +
				"	text-align:center;\n" +
				"}\n" +
				"p {\n" +
				"	line-height: " + this.LineHeight + "\n" +
				"}\n" +
				"hr {\n" +
				"	margin: 35px 0;\n" +
				"}\n" +
				"a {\n" +
				"	margin: 25px 0;\n" +
				"	color: " + this.LinkColor + ";\n" +
				"	text-decoration: none;\n" +
				"	text-shadow: 0px 0px 0px #4169e1;\n" +
				"	transition: 0.5s;\n " +
				"}\n" +
				"	a: hover {\n" +
				"	transition: 1s;\n" +
				"	text-shadow: 0px 0px 1px #4169e1;" +
				"\n}\n" +
				"img, image {\n" +
				"	margin: 0 auto;\n" +
				"	display:block;\n" +
				"}\n" +
				".calibre1 {" +
				"	text-align:center;" +
				"}\n";
			return css;
		}
	}
}