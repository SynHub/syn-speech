using Syn.Logging;
using Syn.Speech.Recognizers;
using Syn.Speech.Results;
using Syn.Speech.Util;
using Syn.Speech.Util.Props;
using IResultListener = Syn.Speech.Decoders.IResultListener;
//PATROLLED + REFACTORED
namespace Syn.Speech.Instrumentation
{
    /// <summary>
    /// Tracks and reports recognition accuracy
    /// </summary>
    public abstract class AccuracyTracker : ConfigurableAdapter, IResultListener, IResetable, IStateListener, IMonitor
    {
        /// <summary>
        /// The property that defines which recognizer to monitor
        /// </summary>
        [S4Component(Type = typeof(Recognizer))]
        public static string PropRecognizer = "recognizer";

        /// <summary>
        /// The property that defines whether summary accuracy information is displayed
        /// </summary>
        [S4Boolean(DefaultValue = true)]
        public static string PropShowSummary = "showSummary";

        /// <summary>
        /// The property that defines whether detailed accuracy information is displayed
        /// </summary>
        [S4Boolean(DefaultValue = true)]
        public static string PropShowDetails = "showDetails";

        /// <summary>
        /// The property that defines whether recognition results should be displayed.
        /// </summary>
        [S4Boolean(DefaultValue = true)]
        public static string PropShowResults = "showResults";

        /// <summary>
        /// The property that defines whether recognition results should be displayed.
        /// </summary>
        [S4Boolean(DefaultValue = true)]
        public static string PropShowAlignedResults = "showAlignedResults";

        /// <summary>
        /// The property that defines whether recognition results should be displayed.
        /// </summary>
        [S4Boolean(DefaultValue = true)]
        public static string PropShowRawResults = "showRawResults";

        // ------------------------------
        // Configuration data
        // ------------------------------
        private string _name;
        private Recognizer _recognizer;
        private bool _showSummary;
        private bool _showDetails;
        private bool _showResults;
        private bool _showAlignedResults;
        private bool _showRaw;

        public AccuracyTracker(Recognizer recognizer, bool showSummary, bool showDetails, bool showResults, bool showAlignedResults, bool showRawResults)
        {
            Aligner = new NISTAlign(false, false);

            InitRecognizer(recognizer);

            _showSummary = showSummary;
            _showDetails = showDetails;
            _showResults = showResults;
            _showAlignedResults = showAlignedResults;

            _showRaw = showRawResults;

            Aligner.SetShowResults(showResults);
            Aligner.SetShowAlignedResults(showAlignedResults);
        }

        public AccuracyTracker()
        {
            Aligner = new NISTAlign(false, false);
        }

        /*
        * (non-Javadoc)
        *
        * @see edu.cmu.sphinx.util.props.Configurable#newProperties(edu.cmu.sphinx.util.props.PropertySheet)
        */
        public override void NewProperties(PropertySheet ps)
        {
            base.NewProperties(ps);
            InitRecognizer((Recognizer)ps.GetComponent(PropRecognizer));

            _showSummary = ps.GetBoolean(PropShowSummary);
            _showDetails = ps.GetBoolean(PropShowDetails);
            _showResults = ps.GetBoolean(PropShowResults);
            _showAlignedResults = ps.GetBoolean(PropShowAlignedResults);

            _showRaw = ps.GetBoolean(PropShowRawResults);

            Aligner.SetShowResults(_showResults);
            Aligner.SetShowAlignedResults(_showAlignedResults);
        }

        private void InitRecognizer(Recognizer newRecognizer)
        {
            if (_recognizer == null)
            {
                _recognizer = newRecognizer;
                _recognizer.AddResultListener(this);
                _recognizer.AddStateListener(this);
            }
            else if (_recognizer != newRecognizer)
            {
                _recognizer.RemoveResultListener(this);
                _recognizer.RemoveStateListener(this);
                _recognizer = newRecognizer;
                _recognizer.AddResultListener(this);
                _recognizer.AddStateListener(this);
            }
        }


        /*
         * (non-Javadoc)
         * 
         * @see edu.cmu.sphinx.instrumentation.Resetable
         */
        public void Reset()
        {
            Aligner.ResetTotals();
        }


        /*
        * (non-Javadoc)
        *
        * @see edu.cmu.sphinx.util.props.Configurable#getName()
        */

        public override string Name
        {
            get { return _name; }
        }


        /**
         * Retrieves the aligner used to track the accuracy stats
         *
         * @return the aligner
         */

        public NISTAlign Aligner { get; private set; }


        /**
         * Shows the complete details.
         *
         * @param rawText the RAW result
         */
        protected void ShowDetails(string rawText)
        {
            if (_showDetails)
            {
                Aligner.PrintSentenceSummary();
                if (_showRaw)
                {
                    this.LogInfo("RAW     " + rawText);
                }
                Aligner.PrintTotalSummary();
            }
        }


        /*
        * (non-Javadoc)
        *
        * @see edu.cmu.sphinx.decoder.ResultListener#newResult(edu.cmu.sphinx.result.Result)
        */
        abstract public void NewResult(Result result);

        public void StatusChanged(Recognizer.State status)
        {
            if (status == Recognizer.State.Deallocated)
            {
                if (_showSummary)
                {
                    this.LogInfo("\n# --------------- Summary statistics ---------");
                    Aligner.PrintTotalSummary();
                }
            }
        }
    }
}
