using System.Globalization;
using Syn.Speech.Logging;
using Syn.Speech.FrontEnds.EndPoint;
using Syn.Speech.Util.Props;
//PATROLLED + REFACTORED
namespace Syn.Speech.FrontEnds.Util
{

    /// <summary>
    /// Dumps the data
    /// </summary>
    public class DataDumper : BaseDataProcessor
    {

        /// <summary>
        /// The property that specifies whether data dumping is enabled
        /// </summary>
        [S4Boolean(DefaultValue = true)]
        public static string PropEnable = "enable";

        /// <summary>
        /// The property that specifies the format of the output.
        /// </summary>
        [S4String(DefaultValue = "0.00000E00;-0.00000E00")]
        public static string PropOutputFormat = "outputFormat";

        /// <summary>
        /// The property that enables the output of signals.
        /// </summary>
        [S4Boolean(DefaultValue = true)]
        public static string PropOutputSignals = "outputSignals";

        // --------------------------
        // Configuration data
        // --------------------------
        private bool _enable;
        private bool _outputSignals;
        private NumberFormatInfo _formatter;

        public DataDumper(bool enable, string format, bool outputSignals)
        {
            //initLogger();
            _formatter = new NumberFormatInfo();
            _outputSignals = outputSignals;
            _enable = enable;
        }

        public DataDumper()
        {

        }


        public override void NewProperties(PropertySheet ps)
        {
            base.NewProperties(ps);

            //logger = ps.getLogger();

            _enable = ps.GetBoolean(PropEnable);
            var format = ps.GetString(PropOutputFormat);
            _formatter = new NumberFormatInfo();
            _outputSignals = ps.GetBoolean(PropOutputSignals);
        }


        /// <summary>
        /// Initializes this DataProcessor. This is typically called after the DataProcessor has been configured.
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();
        }

        /// <summary>
        /// Reads and returns the next Data object from this DataProcessor, return null if there is no more audio data.
        /// </summary>
        /// <returns>
        /// The next Data or <code>null</code> if none is available
        /// </returns>
        public override IData GetData()
        {
            var input = Predecessor.GetData();
            if (_enable)
            {
                DumpData(input);
            }
            return input;
        }

        /// <summary>
        /// Dumps the given input data.
        /// </summary>
        /// <param name="input">The data to dump.</param>
        private void DumpData(IData input)
        {
            this.LogInfo("dumping data...");

            if (input is Signal)
            {
                if (_outputSignals)
                {
                    this.LogInfo("Signal: " + input);
                }
            }
            else if (input is DoubleData)
            {
                var dd = (DoubleData)input;
                var values = dd.Values;
                this.LogInfo("Frame " + values.Length);
                foreach (var val in values)
                {
                    this.LogInfo(' ' + val);
                }
                this.LogInfo("");
            }
            else if (input is SpeechClassifiedData)
            {
                var dd = (SpeechClassifiedData)input;
                var values = dd.Values;
                this.LogInfo("Frame ");
                if (dd.IsSpeech)
                    this.LogInfo('*');
                else
                    this.LogInfo(' ');
                this.LogInfo(" " + values.Length);
                foreach (var val in values)
                {
                    this.LogInfo(' ' + val);
                }
                this.LogInfo("");
            }
            else if (input is FloatData)
            {
                var fd = (FloatData)input;
                var values = fd.Values;
                this.LogInfo("Frame " + values.Length);
                foreach (var val in values)
                {
                    this.LogInfo(' ' + val);
                }
                this.LogInfo("");
            }
        }
    }
}
