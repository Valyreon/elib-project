using System.ComponentModel.DataAnnotations;

namespace Valyreon.Elib.EBookTools
{
    public class StyleSettings
    {
        public static readonly StyleSettings Default = new StyleSettings();

        public StyleSettings(string bgColor, string fgColor, uint fontSize, string font)
        {
            BackgroundColor = bgColor;
            ForegroundColor = fgColor;
            FontSize = fontSize;
            Font = font;
        }

        public StyleSettings()
        {
        }

        [RegularExpression("#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})", ErrorMessage = "Invalid hex color.")]
        public string BackgroundColor { get; set; } = "#f2f1ef";

        public string Font { get; set; } = "Bitter";

        public uint FontSize { get; set; } = 18;

        [RegularExpression("#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})", ErrorMessage = "Invalid hex color.")]
        public string ForegroundColor { get; set; } = "#222222";

        [RegularExpression(@"[0-9]{1}\.[0-9]*", ErrorMessage = "Invalid line height.")]
        public string LineHeight { get; set; } = "1.6";

        [RegularExpression("#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})", ErrorMessage = "Invalid hex color.")]
        public string LinkColor { get; set; } = "#4169e1";

        public int SideMargins { get; set; } = 25;

        /// <summary>
        ///     Generates CSS based on current value of attributes.
        /// </summary>
        /// <returns>String containing css styling.</returns>
        public string GenerateCss()
        {
            var css =
                "html { scroll-behavior: smooth; }" +
                "\nbody {\n" +
                "	margin: 0 " + SideMargins + "%;\n" +
                "	background-color: " + BackgroundColor + ";\n" +
                "	color: " + ForegroundColor + ";\n" +
                "	font-family: " + "\"" + Font + "\"" + ", sans-serif;\n" +
                "	font-size: " + FontSize + "px;\n" +
                "	text-align: justify;\n" +
                "}\n" +
                "h1, h2, h3 {\n" +
                "	text-align:center;\n" +
                "}\n" +
                "p, div {\n" +
                "	line-height: " + LineHeight + "\n" +
                "}\n" +
                "hr {\n" +
                "	margin: 35px 0;\n" +
                "}\n" +
                "a {\n" +
                "	margin: 25px 0;\n" +
                "	color: " + LinkColor + ";\n" +
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
                "}\n" +
                "body img:first-child { max-width:100%; height:auto; }";
            return css;
        }
    }
}
