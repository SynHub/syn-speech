using System;
using System.Text;
//PATROLLED + REFACORED
namespace Syn.Speech.Jsgf.Parser
{
    class JSGFEncoding {
        public String Version;

        public String Encoding;

        public String Locale;

        internal JSGFEncoding(String version, String encoding, String locale) {
            Version = version;
            Encoding = encoding;
            Locale = locale;
        }

        /// <summary>
        /// Extra Method. Gets the Encoding using the encoding's name. 
        /// </summary>
        /// <value>
        /// The get encoding.
        /// </value>
        public Encoding GetEncoding
        {
            get { return System.Text.Encoding.GetEncoding(Encoding); }
        }
    }
}