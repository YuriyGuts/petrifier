using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace YuriyGuts.Petrifier.Persistence.Pnml
{
    /// <summary>
    /// Represents a PNML document containing a definition of Petri Net.
    /// </summary>
    public class PnmlDocument : PersistentDocument
    {
        /// <summary>
        /// Gets the XML document containing the PNML markup.
        /// </summary>
        public XDocument XmlDocument { get; private set; }

        /// <summary>
        /// Loads the specified content into this PNML document.
        /// </summary>
        /// <param name="content"></param>
        public void Load(string content)
        {
            using (StringReader reader = new StringReader(content))
            {
                XDocument document = XDocument.Load(reader);
                Load(document);
            }
        }

        /// <summary>
        /// Loads the specified XML document into this PNML document.
        /// </summary>
        /// <param name="content"></param>
        public void Load(XDocument content)
        {
            XmlDocument = content;
        }

        /// <summary>
        /// Retrieves the content of the document.
        /// </summary>
        /// <returns></returns>
        public override string GetContent()
        {
            // Using XmlWriter and StringBuilder to extract XML content as string.
            XmlWriterSettings settings = new XmlWriterSettings
            {
                OmitXmlDeclaration = true,
                Encoding = Encoding.UTF8,
                ConformanceLevel = ConformanceLevel.Document,
                Indent = true,
            };

            StringBuilder stringBuilder = new StringBuilder(XmlDocument.Declaration + Environment.NewLine);
            using (XmlWriter writer = XmlWriter.Create(stringBuilder, settings))
            {
                XmlDocument.Save(writer);
            }
            
            return stringBuilder.ToString();
        }
    }
}
