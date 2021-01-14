using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using HtmlAgilityPack;
using Valyreon.Elib.EBookTools;

namespace Valyreon.Elib.EbookTools.Epub
{
    public class EpubParser : EbookParser
    {
        private readonly byte[] rawFile;

        public EpubParser(byte[] file)
        {
            StyleSettings = StyleSettings.Default;
            rawFile = file;
        }

        public EpubParser(byte[] file, StyleSettings settings)
        {
            StyleSettings = settings;
            rawFile = file;
        }

        public EpubParser(Stream file)
        {
            StyleSettings = StyleSettings.Default;
            using var ms = new MemoryStream();
            file.CopyTo(ms);
            rawFile = ms.GetBuffer();
        }

        public EpubParser(Stream file, StyleSettings settings)
        {
            StyleSettings = settings;
            using var ms = new MemoryStream();
            file.CopyTo(ms);
            rawFile = ms.GetBuffer();
        }

        /// <summary>
        ///     Parses the epub file and creates ParsedBook filled with all the retrieved metadata and generated html.
        /// </summary>
        public override ParsedBook Parse()
        {
            using var ms = new MemoryStream(rawFile);
            string title, author, publisher, isbn = null;
            byte[] cover;
            using (var zip = new ZipArchive(ms, ZipArchiveMode.Read))
            {
                var doc = GetRootfileDocument(zip);

                // Getting metadata
                title = GetFirstElementByTagName(doc, "dc:title")?.InnerText.Trim();
                author = GetFirstElementByTagName(doc, "dc:creator")?.InnerText.Trim();
                publisher = GetFirstElementByTagName(doc, "dc:publisher")?.InnerText.Trim();
                var identifiers = doc.GetElementsByTagName("dc:identifier");
                foreach (XmlNode identifier in identifiers)
                {
                    var innerValue = identifier.InnerText.Trim();
                    if (identifier.Attributes != null &&
                        (innerValue.Length == 13 || (identifier.Attributes["opf:scheme"]?.Value.Equals("ISBN", StringComparison.OrdinalIgnoreCase) == true)))
                    {
                        isbn = innerValue;
                        break;
                    }
                }

                cover = GetCover(zip);
            }

            return new ParsedBook(title, author, isbn, publisher, cover, ".epub", rawFile);
        }

        public override string GenerateHtml()
        {
            using var ms = new MemoryStream(rawFile);
            using var zip = new ZipArchive(ms, ZipArchiveMode.Read);
            var doc = GetRootfileDocument(zip);
            return FormHtml(doc, zip);
        }

        public byte[] GetCover(ZipArchive zip)
        {
            foreach (var entry in zip.Entries)
            {
                if (entry.Name.IndexOf("cover", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    using var ms = new MemoryStream();
                    entry.Open().CopyTo(ms);
                    return ms.ToArray();
                }
            }

            return null;
        }

        /// <summary>
        ///     Generates epub book as a single html file, and styles it according to StyleSettings.
        /// </summary>
        /// <param name="opf">XmlDocument that contains metadata and other info.</param>
        /// <param name="zip">Opened epub archive.</param>
        /// <returns>Html string.</returns>
        private string FormHtml(XmlDocument opf, ZipArchive zip)
        {
            var builder = new StringBuilder();
            var linkList = new Dictionary<string, string>();

            builder.Append(GenerateHeader()); //Generate header
            builder.Append("<body>\n"); //start body

            var spine = GetFirstElementByTagName(opf, "spine");
            var manifestNode = GetFirstElementByTagName(opf, "manifest");
            var chapterCounter = 0; // we use this to set ID attributes on each div that we are going to enclose a chapter into
            foreach (XmlNode child in spine.ChildNodes)
            {
                if (child.Attributes != null)
                {
                    var id = child.Attributes["idref"]
                        ?.Value; // we find the ID of the xml tag that contains the path to the file of the chapter in epub zip manifest block
                    var item = manifestNode.ChildNodes
                        .Cast<XmlNode>()
                        .FirstOrDefault(node =>
                            node?.Attributes?["id"]?.Value == id); // now we get the xmlnode with that id

                    if (item?.Attributes != null)
                    {
                        var itemPath = item.Attributes["href"]?.Value; // we get the file it points to
                        var itemFile =
                            zip.Entries.FirstOrDefault(e =>
                                e.FullName.EndsWith(itemPath ?? string.Empty)); // now we get the file
                        if (itemFile != null)
                        {
                            var reader = new StreamReader(itemFile.Open());
                            var htmlString = reader.ReadToEnd(); // read the html from file

                            var htmlDoc = new HtmlDocument();
                            htmlDoc.LoadHtml(htmlString); // parse the html string with HttpAgilityPack
                            var bodyContent = htmlDoc.DocumentNode.SelectSingleNode("//body"); // get the <body> node

                            var chapterId = "chapter" + chapterCounter;
                            builder.Append("<div id=").Append(chapterId).Append(">\n"); // enclose the chapter in <div> with id
                            var trimmedFileName = itemFile.Name.TrimStart('.', '/', '\\');
                            linkList.Add(trimmedFileName,
                                "#" + chapterId); // remember the chapter name and its id so we can update the links later
                            builder.Append(bodyContent.InnerHtml);
                        }
                    }
                }

                builder.Append("</div>\n");
                builder.Append("<hr/>\n");
                chapterCounter++;
            }

            builder.Append("</body>");
            var finalDoc = new HtmlDocument();
            finalDoc.LoadHtml(builder.ToString());
            UpdateLinks(finalDoc, linkList);
            EmbedImages(finalDoc, zip);
            return finalDoc.DocumentNode.OuterHtml;
        }

        /// <summary>
        ///     Gets the XML file containing all metadata and other important things.
        /// </summary>
        /// <param name="zip">Opened epub archive.</param>
        /// <returns>Loaded XmlDocument.</returns>
        private static XmlDocument GetRootfileDocument(ZipArchive zip)
        {
            // Reading container.xml file
            var containerFile = zip.GetEntry("META-INF/container.xml");
            if (containerFile != null)
            {
                var reader = new StreamReader(containerFile.Open());
                var containerText = reader.ReadToEnd();
                var doc = new XmlDocument();
                doc.LoadXml(containerText);
                var xmlAttributeCollection = GetFirstElementByTagName(doc, "rootfile")?.Attributes;
                if (xmlAttributeCollection != null)
                {
                    var opfFilePath = xmlAttributeCollection["full-path"]?.Value;

                    // Parsing content.opf file
                    var opfFile = zip.GetEntry(opfFilePath ?? throw new InvalidOperationException());
                    if (opfFile != null)
                    {
                        reader = new StreamReader(opfFile.Open());
                    }
                }

                var contentText = reader.ReadToEnd();
                doc = new XmlDocument();
                doc.LoadXml(contentText);
                return doc;
            }

            return null;
        }

        /// <summary>
        ///     Returns the first XmlNode whose tag matches the string parameter from XmlDocument parameter.
        /// </summary>
        /// <param name="doc">Document to search.</param>
        /// <param name="tag">Tag to find.</param>
        /// <returns></returns>
        private static XmlNode GetFirstElementByTagName(XmlDocument doc, string tag)
        {
            var list = doc?.GetElementsByTagName(tag);
            var node = list?.Item(0);
            return node;
        }

        private static void EmbedImages(HtmlDocument htmlDoc, ZipArchive zip)
        {
            var allImages = htmlDoc.DocumentNode.SelectNodes("//img | //image");

            if (allImages == null)
            {
                return;
            }

            foreach (var node in allImages)
            {
                if (node.Attributes.Contains("src"))
                {
                    ReplaceSourceWithImage(node, "src", zip);
                }
                else if (node.Attributes.Contains("xlink:href"))
                {
                    ReplaceSourceWithImage(node, "xlink:href", zip);
                }
                else
                {
                    node.Remove();
                }
            }
        }

        private static void ReplaceSourceWithImage(HtmlNode node, string attributeName, ZipArchive zip)
        {
            var href = node.Attributes[attributeName].Value;
            href = href.TrimStart('.', '\\', '/').Trim();
            foreach (var entry in zip.Entries)
            {
                if (entry.FullName.Contains(href))
                {
                    using var ms = new MemoryStream();
                    entry.Open().CopyTo(ms);
                    var imageString = "data:image/" + Path.GetExtension(entry.Name) + ";base64," +
                                         ImageToBase64String(ms.GetBuffer());
                    node.Attributes[attributeName].Value = imageString;
                    break;
                }
            }
        }

        /// <summary>
        ///     Updates the links in the new html file so they point to right chapters. If links don't point to anything
        ///     or the dictionary does not contain that link, it deletes them.
        /// </summary>
        /// <param name="httpDoc"></param>
        /// <param name="dict">Dictionary where Key is file path to chapter, and Value is chapter ID in new HTML document.</param>
        private static void UpdateLinks(HtmlDocument httpDoc, Dictionary<string, string> dict)
        {
            var allLinks = httpDoc.DocumentNode.SelectNodes("//a");
            if (allLinks != null)
            {
                foreach (var link in allLinks)
                {
                    if (link.Attributes.Contains("href"))
                    {
                        var href = link.Attributes["href"].Value.Trim();
                        var cleanedHref = Regex.Replace(href, "#\\w*", "");
                        cleanedHref = Path.GetFileName(cleanedHref);
                        if (dict.ContainsKey(cleanedHref))
                        {
                            link.Attributes["href"].Value = dict[cleanedHref];
                        }
                        else if (href.StartsWith(".") || href.StartsWith("/") || href.StartsWith("\\"))
                        {
                            link.Name = "p"; // if link doesnt match any chapter, replace tag with p
                        }
                    }
                }
            }
        }

        private static string ImageToBase64String(byte[] image)
        {
            // Convert byte[] to Base64 String
            var base64String = Convert.ToBase64String(image);
            return base64String;
        }
    }
}
