using System.IO;
using System.Xml;

namespace Syn.Speech.Helper
{
    public class XMLReader
    {
        public delegate void startElement(URL uri, string localName, string qName, Attributes attributes);
        public delegate void characters(char[] ch, int start, int length);
        public delegate void endElement(URL uri, string localName, string qName);
        public delegate void error(SAXParseException exception);
        public delegate void setDocumentLocator(ILocator locator) ;

        private startElement startElementHandler;
        private characters charactersHandler;
        private endElement endElementHandler;
        private error errorHandler;
        private setDocumentLocator setDocumentLocatorHandler;

        public XMLReader()
        { 
        }

        public void setContentHandler(DefaultHandler handler)
        {
            startElementHandler += handler.StartElement;
            charactersHandler += handler.Characters;
            endElementHandler += handler.EndElement;
        }

        public void setErrorHandler(DefaultHandler handler)
        {
            errorHandler += handler.Error;
        }

        public void parse(URL url)
        {
            TextReader textReader = null;
            if (url.Type == URLType.Resource)
            {
                textReader = new StringReader(url.Content);
            }
            else if(url.Type == URLType.Path)
            {
                textReader = new StreamReader(url.Path);
            }
            
            using (XmlReader reader = XmlReader.Create(textReader))
            {
                while (reader.Read())
                {
                    switch (reader.NodeType)
                    {
                        case XmlNodeType.Element:
                            if (startElementHandler == null)
                                continue;
                            bool isEmptyElement = reader.IsEmptyElement;
                            string readerLocalName = reader.LocalName;
                            string readerName = reader.Name;
                            // if go forward the Name is overwrited.
                            var attributes = new Attributes(reader.AttributeCount);
                            while (reader.MoveToNextAttribute()) 
                            {
                                attributes.Add(reader.Name, reader.Value);
                            }
                            startElementHandler(url, readerLocalName, readerName, attributes);
                            
                            if (isEmptyElement)
                            {
                                if (endElementHandler == null) continue;
                                endElementHandler(url, readerLocalName, readerName);                            
                            }
                            break;
                        case XmlNodeType.Text:
                            if (charactersHandler != null)
                                charactersHandler(reader.Value.ToCharArray(), 0, reader.Value.Length);
                            break;
                        //case XmlNodeType.XmlDeclaration:
                        //case XmlNodeType.ProcessingInstruction:
                        //    writer.WriteProcessingInstruction(reader.Name, reader.Value);
                        //    break;
                        //case XmlNodeType.Comment:
                        //    writer.WriteComment(reader.Value);
                        //    break;
                        case XmlNodeType.EndElement:
                            if (endElementHandler != null)
                                endElementHandler(url, reader.LocalName, reader.Name);
                            break;
                    }

                }
            }
        }
    }
}
