using HtmlAgilityPack;

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

namespace EbookTools.Epub
{
    public class EpubParser : EbookParser
    {
        private readonly byte[] _rawFile;

        public EpubParser(byte[] file)
        {
            StyleSettings = StyleSettings.Default;
            _rawFile = file;
        }

        public EpubParser(byte[] file, StyleSettings settings)
        {
            StyleSettings = settings;
            _rawFile = file;
        }

        public EpubParser(Stream file)
        {
            StyleSettings = StyleSettings.Default;
            MemoryStream ms = new MemoryStream();
            file.CopyTo(ms);
            _rawFile = ms.GetBuffer();
        }

        public EpubParser(Stream file, StyleSettings settings)
        {
            StyleSettings = settings;
            MemoryStream ms = new MemoryStream();
            file.CopyTo(ms);
            _rawFile = ms.GetBuffer();
        }

        /// <summary>
        /// Parses the epub file and creates ParsedBook filled with all the retrieved metadata and generated html.
        /// </summary>
        public override ParsedBook Parse()
        {
            MemoryStream ms = new MemoryStream(_rawFile);
            string title, author, publisher, isbn = null, htmlBook;
            XmlDocument doc = null;
            byte[] cover;
            using (var zip = new ZipArchive(ms, ZipArchiveMode.Read))
            {
                doc = GetRootfileDocument(zip);

                // Getting metadata
                title = GetFirstElementByTagName(doc, "dc:title")?.InnerText.Trim();
                author = GetFirstElementByTagName(doc, "dc:creator")?.InnerText.Trim();
                publisher = GetFirstElementByTagName(doc, "dc:publisher")?.InnerText.Trim();
                var identifiers = doc.GetElementsByTagName("dc:identifier");
                foreach (XmlNode identifier in identifiers)
                {
                    if (identifier.Attributes["opf:scheme"].Value.ToUpper().Equals("ISBN"))
                    {
                        isbn = identifier.InnerText.Trim();
                        break;
                    }
                }
                htmlBook = this.GenerateHtml(doc, zip);
                cover = GetCover(zip);
            }
            return new ParsedBook(title, author, isbn, publisher, htmlBook, cover, ".epub", _rawFile);
        }

        public byte[] GetCover(ZipArchive zip)
        {
            foreach (var entry in zip.Entries)
            {
                if (entry.Name.ToLower().Contains("cover"))
                {
                    MemoryStream ms = new MemoryStream();
                    entry.Open().CopyTo(ms);
                    return ms.ToArray();
                }
            }
            return null;
        }

        /// <summary>
        /// Generates epub book as a single html file, and styles it according to StyleSettings.
        /// </summary>
        /// <param name="opf">XmlDocument that contains metadata and other info.</param>
        /// <param name="zip">Opened epub archive.</param>
        /// <returns>Html string.</returns>
        private string GenerateHtml(XmlDocument opf, ZipArchive zip)
        {
            StringBuilder builder = new StringBuilder();
            Dictionary<string, string> linkList = new Dictionary<string, string>();

            builder.Append(GenerateHeader()); //Generate header
            builder.Append("<body>\n"); //start body

            XmlNode spine = GetFirstElementByTagName(opf, "spine");
            XmlNode manifestNode = GetFirstElementByTagName(opf, "manifest");
            int chapterCounter = 0; // we use this to set ID attributes on each div that we are going to enclose a chapter into
            foreach (XmlNode child in spine.ChildNodes)
            {
                string id = child.Attributes["idref"]?.Value; // we find the ID of the xml tag that contains the path to the file of the chapter in epub zip manifest block
                XmlNode item = manifestNode.ChildNodes.Cast<XmlNode>()
                    .Where(node => node?.Attributes?["id"]?.Value == id)
                    .FirstOrDefault(); // now we get the xmlnode with that id

                string itemPath = item.Attributes["href"]?.Value; // we get the file it points to
                var itemFile = zip.Entries.Where(e => e.FullName.EndsWith(itemPath)).FirstOrDefault(); // now we get the file
                StreamReader reader = new StreamReader(itemFile.Open());
                string htmlString = reader.ReadToEnd(); // read the html from file

                var htmlDoc = new HtmlDocument();
                htmlDoc.LoadHtml(htmlString); // parse the html string with HttpAgilityPack
                HtmlNode bodyContent = htmlDoc.DocumentNode.SelectSingleNode("//body"); // get the <body> node

                string chapterId = "chapter" + chapterCounter;
                builder.Append("<div id=" + chapterId + ">\n"); // enclose the chapter in <div> with id
                string trimmedFileName = itemFile.Name.TrimStart(new char[] { '.', '/', '\\' });
                linkList.Add(trimmedFileName, "#" + chapterId); // remember the chapter name and its id so we can update the links later
                builder.Append(bodyContent.InnerHtml);
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
        /// Gets the XML file containing all metadata and other important things.
        /// </summary>
        /// <param name="zip">Opened epub archive.</param>
        /// <returns>Loaded XmlDocument.</returns>
        private static XmlDocument GetRootfileDocument(ZipArchive zip)
        {
            XmlDocument doc;

            // Reading container.xml file
            var containerFile = zip.GetEntry("META-INF/container.xml");
            StreamReader reader = new StreamReader(containerFile.Open());
            string containerText = reader.ReadToEnd();
            doc = new XmlDocument();
            doc.LoadXml(containerText);
            string opfFilePath = GetFirstElementByTagName(doc, "rootfile")?.Attributes["full-path"]?.Value;

            // Parsing content.opf file
            var opfFile = zip.GetEntry(opfFilePath);
            reader = new StreamReader(opfFile.Open());
            string contentText = reader.ReadToEnd();
            doc = new XmlDocument();
            doc.LoadXml(contentText);
            return doc;
        }

        /// <summary>
        /// Returns the first XmlNode whose tag matches the string parameter from XmlDocument parameter.
        /// </summary>
        /// <param name="doc">Document to search.</param>
        /// <param name="tag">Tag to find.</param>
        /// <returns></returns>
        private static XmlNode GetFirstElementByTagName(XmlDocument doc, string tag)
        {
            XmlNodeList list = doc?.GetElementsByTagName(tag);
            XmlNode node = list?.Item(0);
            return node;
        }

        private static void EmbedImages(HtmlDocument htmlDoc, ZipArchive zip)
        {
            var all_images = htmlDoc.DocumentNode.SelectNodes("//img | //image");

            if (all_images != null)
            {
                foreach (var node in all_images)
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
        }

        private static void ReplaceSourceWithImage(HtmlNode node, string attributeName, ZipArchive zip)
        {
            string href = node.Attributes[attributeName].Value;
            href = href.TrimStart(new char[] { '.', '\\', '/' }).Trim();
            foreach (var entry in zip.Entries)
            {
                if (entry.FullName.Contains(href))
                {
                    MemoryStream ms = new MemoryStream();
                    entry.Open().CopyTo(ms);
                    string imageString = "data:image/" + Path.GetExtension(entry.Name) + ";base64," + ImageToBase64String(ms.GetBuffer());
                    node.Attributes[attributeName].Value = imageString;
                    break;
                }
            }
        }

        /// <summary>
        /// Updates the links in the new html file so they point to right chapters. If links don't point to anything
        /// or the dictionary does not contain that link, it deletes them.
        /// </summary>
        /// <param name="httpDoc"></param>
        /// <param name="dict">Dictionary where Key is file path to chapter, and Value is chapter ID in new HTML document.</param>
        private static void UpdateLinks(HtmlDocument httpDoc, Dictionary<string, string> dict)
        {
            var all_links = httpDoc.DocumentNode.SelectNodes("//a");
            if (all_links != null)
            {
                foreach (var link in all_links)
                {
                    if (link.Attributes.Contains("href"))
                    {
                        string href = link.Attributes["href"].Value.Trim();
                        string cleanedHref = Regex.Replace(href, "#\\w*", "");
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
            string base64String = Convert.ToBase64String(image);
            return base64String;
        }
    }
}