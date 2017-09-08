using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using Syn.Logging;
using Syn.Speech.Helper;
//REFACTORED
namespace Syn.Speech.Util.Props
{
    /// <summary>
    /// A SAX XML Handler implementation that builds up the map of raw property data objects
    /// </summary>
    public class ConfigHandler : DefaultHandler
    {
        protected RawPropertyData RPD;
        protected ILocator Locator;
        protected List<String> ItemList;
        protected string ItemListName;
        protected StringBuilder CurItem;

        protected  HashMap<String, RawPropertyData> RPDMap;
        protected HashMap<String, String> GlobalProperties;

        private bool _replaceDuplicates;
        private readonly URL _baseUrl;

        public ConfigHandler(HashMap<String, RawPropertyData> rpdMap, HashMap<String, String> globalProperties, bool replaceDuplicates, URL baseUrl)
        {
            RPDMap = rpdMap;
            GlobalProperties = globalProperties;
            _replaceDuplicates = replaceDuplicates;
            _baseUrl = baseUrl;
        }

        public ConfigHandler(HashMap<String, RawPropertyData> rpdMap, HashMap<String, String> globalProperties)
            : this(rpdMap, globalProperties, false, null)
        {
            
        }

        public override void StartElement(URL uri, string localName, string qName, Attributes attributes)
        {
            if (qName.Equals("config"))
            {
                // test if this configuration extends another one
                var extendedConfigName = attributes.getValue("extends");
                if (extendedConfigName != null)
                {
                    MergeConfigs(extendedConfigName, true);
                    _replaceDuplicates = true;
                }
            }
            else if (qName.Equals("include"))
            {
                var includeFileName = attributes.getValue("file");
                MergeConfigs(includeFileName, false);
            }
            else if (qName.Equals("extendwith"))
            {
                var includeFileName = attributes.getValue("file");
                MergeConfigs(includeFileName, true);
            }
            else if (qName.Equals("component"))
            {
                var curComponent = attributes.getValue("name");
                var curType = attributes.getValue("type");
                if (RPDMap.Get(curComponent) != null && !_replaceDuplicates)
                {
                    throw new XmlException("duplicate definition for " + curComponent);
                }
                RPD = new RawPropertyData(curComponent, curType);
            }
            else if (qName.Equals("property"))
            {
                var name = attributes.getValue("name");
                var value = attributes.getValue("value");
                if (attributes.getLength() != 2 || name == null || value == null)
                {
                    throw new XmlException("property element must only have 'name' and 'value' attributes");
                }
                if (RPD == null)
                {
                    // we are not in a component so add this to the global
                    // set of symbols
                    //                    String symbolName = "${" + name + "}"; // why should we warp the global props here
                    GlobalProperties.Put(name, value);
                }
                else if (RPD.Contains(name) && !_replaceDuplicates)
                {
                    throw new XmlException("Duplicate property: " + name);
                }
                else
                {
                    RPD.Add(name, value);
                }
            }
            else if (qName.Equals("propertylist"))
            {
                ItemListName = attributes.getValue("name");
                if (attributes.getLength() != 1 || ItemListName == null)
                {
                    throw new XmlException("list element must only have the 'name'  attribute");
                }
                ItemList = new List<String>();
            }
            else if (qName.Equals("item"))
            {
                if (attributes.getLength() != 0)
                {
                    throw new XmlException("unknown 'item' attribute");
                }
                CurItem = new StringBuilder();
            }
            else
            {
                throw new XmlException("Unknown element '" + qName + '\'');
            }
        }

        /// <summary>
        /// Receive notification of character data inside an element.
        /// By default, do nothing. Application writers may override this method to take specific actions for each chunk of character data (such as adding the data to a node or buffer, or printing it to a file).
        /// </summary>
        /// <param name="buf"></param>
        /// <param name="offset"></param>
        /// <param name="len"></param>
        public override void Characters(char[] buf, int offset, int len)
        {
            if (CurItem != null) 
            {
                CurItem.Append(buf, offset, len);
            }
        }

        /// <summary>
        ///  Receive notification of the end of an element.
        ///  By default, do nothing. Application writers may override this method in a subclass to take specific actions at the end of each element (such as finalising a tree node or writing output to a file).
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="localName"></param>
        /// <param name="qName"></param>
        public override void EndElement(URL uri, string localName, string qName)
        {
            if (qName.Equals("component")) 
            {
                RPDMap.Put(RPD.Name, RPD);
                RPD = null;
            } 
            else if (qName.Equals("property")) 
            {
                // nothing to do
            } 
            else if (qName.Equals("propertylist")) 
            {
                if (RPD.Contains(ItemListName)) 
                {
                    throw new Exception("Duplicate property: " + ItemListName/*, locator*/);
                }
                RPD.Add(ItemListName, ItemList);
                ItemList = null;
            } 
            else if (qName.Equals("item")) 
            {
                ItemList.Add(CurItem.ToString().Trim());
                CurItem = null;
            }
        }

        public override void Error(SAXParseException exception)
        {
            this.LogError(exception.Message);
        }

        /// <summary>
        /// Receive a Locator object for document events. 
        /// </summary>
        /// <param name="locator"></param>
        public void SetDocumentLocator(ILocator locator) 
        {
            Locator = locator;
        }

        private void MergeConfigs(String configFileName, Boolean replaceDuplicates) 
        {
            try
            {
                var parentPath = _baseUrl.File.DirectoryName;
                var fileUrl = new URL(Path.Combine(parentPath, configFileName));
                this.LogInfo((replaceDuplicates ? "extending" : "including") + " config:" + fileUrl);
                var saxLoader = new SaxLoader(fileUrl, GlobalProperties, RPDMap, replaceDuplicates);
                saxLoader.Load();
            } 
            catch (IOException e) 
            {
                throw new SystemException("Error while processing <include file=\"" + configFileName + "\">: " + e, e);
            }
        }
    }
}
