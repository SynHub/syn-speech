//REFACTORED
namespace Syn.Speech.Helper
{
    public abstract class DefaultHandler
    {
        public abstract void StartElement(URL uri, string localName, string qName, Attributes attributes);

        public abstract void Characters(char[] buf, int offset, int len);

        public abstract void EndElement(URL uri, string localName, string qName);

        public abstract void Error(SAXParseException exception);
    }
}
