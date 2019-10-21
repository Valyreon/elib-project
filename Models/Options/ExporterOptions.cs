using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.Options
{
    public class ExporterOptions
    {
        public enum Format
        {
            Epub,
            Mobi
        }

        public Format ExportEpub { get; private set; } = Format.Epub;

        public Format ExportMobi { get; private set; } = Format.Mobi;

        public static string GetFormat(Format format)
        {
            switch(format)
            {
                case Format.Epub:
                    return ".epub";
                case Format.Mobi:
                    return ".mobi";
                default:
                    return "";
            }
        }
    }
}
