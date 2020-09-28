using System;
using System.IO;
using System.Xml;
using Syn.Speech.Helper;
//PATROLLED + REFACTORED
namespace Syn.Speech.Util.Props
{
    
    /// <summary>
    /// Loads configuration from an XML file
    /// </summary>
    public class SaxLoader
    {
        private readonly URL _url;
        private readonly HashMap<String, RawPropertyData> _rpdMap;
        private readonly HashMap<String, String> _globalProperties;
        private readonly Boolean _replaceDuplicates;

        /// <summary>
        /// Creates a loader that will load from the given location 
        /// </summary>
        /// <param name="url">the location to load</param>
        /// <param name="globalProperties">the map of global properties</param>
        /// <param name="initRPD"></param>
        /// <param name="replaceDuplicates"></param>
        public SaxLoader(URL url, HashMap<String, String> globalProperties, HashMap<String, RawPropertyData> initRPD, Boolean replaceDuplicates)
        {
            _url = url;
            _globalProperties = globalProperties;
            _replaceDuplicates = replaceDuplicates;
            _rpdMap = initRPD ?? new HashMap<String, RawPropertyData>();
        }

        /// <summary>
        /// Creates a loader that will load from the given location 
        /// </summary>
        /// <param name="url">the location to load</param>
        /// <param name="globalProperties">the map of global properties</param>
        public SaxLoader(URL url, HashMap<String, String> globalProperties)
            : this(url, globalProperties, null, false)
        {

        }

        /// <summary>
        /// Loads a set of configuration data from the location 
        /// </summary>
        /// <returns>A map keyed by component name containing RawPropertyData objects.</returns>
        public HashMap<String, RawPropertyData> Load()
        {
            try
            {
                var xmlReader = new  XMLReader();
                var handler = new ConfigHandler(_rpdMap, _globalProperties, _replaceDuplicates, _url);
                xmlReader.setContentHandler(handler);
                xmlReader.parse(_url);
            }
            catch (XmlException e)
            {
                var msg = "Error while parsing line " + e.LineNumber + " of " + _url + ": " + e.Message;
                throw new IOException(msg);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }

            return _rpdMap;
        }
    }
}
