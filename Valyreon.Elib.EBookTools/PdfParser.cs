using System;
using System.Collections.Generic;
using System.Linq;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Xobject;

namespace Valyreon.Elib.EBookTools
{
    public class PdfParser : EbookParser
    {
        private readonly string filePath;

        public PdfParser(string filePath)
        {
            this.filePath = filePath;
        }

        public override string GenerateHtml()
        {
            throw new NotImplementedException();
        }

        public override ParsedBook Parse()
        {
            using var pdfReader = new PdfReader(filePath);
            using var pdfDocument = new PdfDocument(pdfReader);

            var documentInfo = pdfDocument.GetDocumentInfo();
            var author = documentInfo.GetAuthor().Clean();
            var title = documentInfo.GetTitle();
            var publisher = documentInfo.GetCreator();

            var allImages = GetFirstPageImages(pdfDocument);

            return new ParsedBook
            {
                Path = filePath,
                Authors = string.IsNullOrWhiteSpace(author) ? new List<string>() : new List<string> { author },
                Title = title.Clean(),
                Publisher = publisher.Clean(),
                Cover = allImages.OrderByDescending(c => c.Length).FirstOrDefault()
            };
        }

        private static List<byte[]> GetFirstPageImages(PdfDocument pdfDocument)
        {
            var firstpage = pdfDocument.GetFirstPage();
            var result = new List<byte[]>();
            Stack<PdfObject> stack = new();
            stack.Push(firstpage.GetPdfObject());
            while (stack.Any())
            {
                var entry = stack.Pop();

                if (entry.IsIndirect())
                {
                    entry = pdfDocument.GetPdfObject(entry.GetIndirectReference().GetObjNumber());
                }

                if (entry.IsDictionary())
                {
                    var dict = entry as PdfDictionary;
                    foreach (var c in dict.Values())
                    {
                        stack.Push(c);
                    }
                }
                else if (entry.IsStream())
                {
                    var pdfStream = entry as PdfStream;
                    if (pdfStream.ContainsKey(PdfName.Subtype))
                    {
                        var subType = pdfStream.GetAsName(PdfName.Subtype).GetValue();
                        if (subType == PdfName.Image.GetValue()
                            || subType == PdfName.ImageMask.GetValue()
                            || subType == PdfName.StampImage.GetValue()
                        )
                        {
                            var imageObj = PdfXObject.MakeXObject(pdfStream) as PdfImageXObject;
                            result.Add(imageObj.GetImageBytes());
                        }
                    }
                }
            }

            return result;
        }
    }
}
