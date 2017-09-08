using System;
using Syn.Logging;
using Syn.Speech.Recognizers;
using Syn.Speech.Results;
using Syn.Speech.Util.Props;
using IResultListener = Syn.Speech.Decoders.IResultListener;
//PATROLLED + REFACTORED
namespace Syn.Speech.Instrumentation
{

    /// <summary>
    /// Monitors a recognizer for memory usage
    /// </summary>
    public class MemoryTracker : ConfigurableAdapter, IStateListener, IMonitor, IResultListener
    {

        /// <summary>
        /// The property that defines which recognizer to monitor.
        /// </summary>
        [S4Component(Type = typeof(Recognizer))]
        public static string PropRecognizer = "recognizer";

        /// <summary>
        /// The property that defines whether summary accuracy information is displayed.
        /// </summary>
        [S4Boolean(DefaultValue = true)]
        public static string PropShowSummary = "showSummary";

        /// <summary>
        /// The property that defines whether detailed accuracy information is displayed.
        /// </summary>
        [S4Boolean(DefaultValue = true)]
        public static string PropShowDetails = "showDetails";

        private const string MemFormat = "#.## Mb";
        // ------------------------------
        // Configuration data
        // ------------------------------
        private string _name;
        private Recognizer _recognizer;
        private bool _showSummary;
        private bool _showDetails;
        private float _maxMemoryUsed;
        private int _numMemoryStats;
        private float _avgMemoryUsed;

        public MemoryTracker(Recognizer recognizer, bool showSummary, bool showDetails)
        {
            InitRecognizer(recognizer);
            _showSummary = showSummary;
            _showDetails = showDetails;
        }

        public MemoryTracker()
        {

        }

        /*
        * (non-Javadoc)
        *
        * @see edu.cmu.sphinx.util.props.Configurable#newProperties(edu.cmu.sphinx.util.props.PropertySheet)
        */
        public override void NewProperties(PropertySheet ps)
        {
            base.NewProperties(ps);
            Recognizer newRecognizer = (Recognizer)ps.GetComponent(
                    PropRecognizer);
            InitRecognizer(newRecognizer);
            _showSummary = ps.GetBoolean(PropShowSummary);
            _showDetails = ps.GetBoolean(PropShowDetails);
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



        public override string Name
        {
            get { return _name; }
        }


        /// <summary>
        /// Shows memory usage
        /// </summary>
        /// <param name="show">if set to <c>true</c> [show].</param>
        private void CalculateMemoryUsage(bool show)
        {
            float totalMem = Environment.WorkingSet / (1024.0f * 1024.0f);
            //float freeMem = Environment. / (1024.0f * 1024.0f);
            float usedMem = totalMem; //- freeMem; //TODO: FIND feasible alternative to get the available RAM
            if (usedMem > _maxMemoryUsed)
            {
                _maxMemoryUsed = usedMem;
            }

            _numMemoryStats++;
            _avgMemoryUsed = ((_avgMemoryUsed * (_numMemoryStats - 1)) + usedMem)
                    / _numMemoryStats;

            if (show)
            {
                this.LogInfo("   Mem  Total: " + totalMem.ToString(MemFormat));
                this.LogInfo("   Used: This: " + usedMem.ToString(MemFormat) + "  "
                        + "Avg: " + _avgMemoryUsed.ToString(MemFormat) + "  " + "Max: "
                        + _maxMemoryUsed.ToString(MemFormat));
            }
        }

        public void NewResult(Result result)
        {
            if (result.IsFinal())
            {
                CalculateMemoryUsage(_showDetails);
            }
        }

        public void StatusChanged(Recognizer.State status)
        {
            if (status == Recognizer.State.Deallocated)
            {
                CalculateMemoryUsage(_showSummary);
            }
        }
    }
}
