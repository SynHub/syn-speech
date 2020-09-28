using System;
using System.IO;
using Syn.Speech.Logging;
using Syn.Speech.Helper;
using Syn.Speech.Util.Props;
using Syn.Speech.Util.Props.Tools;
//PATROLLED + REFACTORED
namespace Syn.Speech.Instrumentation
{
    /// <summary>
    /// Shows the configuration currently in use. This monitor is typically added as a recognition monitor such that the configuration is shown immediately after the recognizer is allocated.
    /// </summary>
    public class ConfigMonitor : IRunnable, IMonitor
    {
        /// <summary>
        /// The property that is used to indicate whether or not this monitor should show the current configuration.
        /// </summary>
        [S4Boolean(DefaultValue = false)]
        public const string PropShowConfig = "showConfig";

        /// <summary>
        /// The property that is used to indicate whether or not this monitor should dump the configuration in an HTML document
        /// </summary>
        [S4Boolean(DefaultValue = false)]
        public const string PropShowConfigAsHtml = "showConfigAsHTML";

        /// <summary>
        /// The property that is used to indicate whether or not this monitor should dump the configuration in an GDL document
        /// </summary>
        [S4Boolean(DefaultValue = false)]
        public const String PropShowConfigAsGdl = "showConfigAsGDL";

        /// <summary>
        /// The property that is used to indicate whether or not this monitor should save the configuration in an XML document
        /// </summary>
        [S4Boolean(DefaultValue = false)]
        public const String PropSaveConfigAsXML = "saveConfigAsXML";

        [S4String(Mandatory = false)]
        public const String PropOutfile = "file";

        // -------------------------
        // Configuration data
        // -------------------------
        private bool _showConfig;
        private bool _showHtml = true;
        private bool _saveXML;
        private bool _showGdl = true;

        //private Logger logger;
        private ConfigurationManager _cm;

        private String _htmlPath = "config.html";
        private String _gdlPath = "config.gdl";
        private String _xmlPath = "config.xml";

        public void NewProperties(PropertySheet ps)
        {
            //logger = ps.getLogger();
            _cm = ConfigurationManagerUtils.GetPropertyManager(ps);

            _showConfig = ps.GetBoolean(PropShowConfig);
            _showHtml = ps.GetBoolean(PropShowConfigAsHtml);
            _showGdl = ps.GetBoolean(PropShowConfigAsGdl);
            _saveXML = ps.GetBoolean(PropSaveConfigAsXML);

            if (ps.GetString(PropOutfile) != null)
            {
                var outFile = new FileInfo(ps.GetString(PropOutfile));
                if (outFile.Directory != null && outFile.Directory.Exists)
                {
                    //TODO: Check Behaviour
                    _htmlPath = outFile.FullName;
                    _gdlPath = outFile.FullName;
                    _xmlPath = outFile.FullName;
                }
            }
        }

        public void Run()
        {
            if (_showConfig)
            {
                ConfigurationManagerUtils.ShowConfig(_cm);
            }

            if (_showHtml)
            {
                try
                {
                    HTMLDumper.ShowConfigAsHTML(_cm, "foo.html");
                }
                catch (IOException e)
                {
                    this.LogWarning("Can't open " + _htmlPath + ' ' + e);
                }
            }

            if (_showGdl)
            {
                try
                {
                    GDLDumper.ShowConfigAsGDL(_cm, _gdlPath);
                }
                catch (IOException e)
                {
                    this.LogWarning("Can't open " + _gdlPath + ' ' + e);
                }
            }

            if (_saveXML)
            {
                ConfigurationManagerUtils.Save(_cm, new FileInfo(_xmlPath));
            }
        }
    }
}
