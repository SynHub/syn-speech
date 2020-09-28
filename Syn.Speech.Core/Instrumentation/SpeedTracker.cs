using Syn.Speech.Logging;
using Syn.Speech.FrontEnds;
using Syn.Speech.FrontEnds.EndPoint;
using Syn.Speech.Helper;
using Syn.Speech.Recognizers;
using Syn.Speech.Results;
using Syn.Speech.Util;
using Syn.Speech.Util.Props;
using IResultListener = Syn.Speech.Decoders.IResultListener;
//PATROLLED + REFACTORED
namespace Syn.Speech.Instrumentation
{

    /// <summary>
    /// Monitors a recognizer for speed
    /// </summary>
public class SpeedTracker:ConfigurableAdapter,IResultListener, IResetable,IStateListener,ISignalListener,IMonitor {

    /// <summary>
    /// The property that defines which recognizer to monitor.
    /// </summary>
    [S4Component(Type = typeof(Recognizer))]
    public  static string PropRecognizer = "recognizer";

    /// <summary>
    /// The property that defines which frontend to monitor.
    /// </summary>
    [S4Component(Type = typeof(FrontEnds.FrontEnd))]
    public  static string PropFrontend = "frontend";

    /// <summary>
    /// The property that defines whether summary accuracy information is displayed.
    /// </summary>
    [S4Boolean(DefaultValue = true)]
    public  static string PropShowSummary = "showSummary";

    /// <summary>
    /// The property that defines whether detailed accuracy information is displayed.
    /// </summary>
    [S4Boolean(DefaultValue = true)]
    public  static string PropShowDetails = "showDetails";

    /// <summary>
    /// The property that defines whether detailed response information is displayed.
    /// </summary>
    [S4Boolean(DefaultValue = false)]
    public  static string PropShowResponseTime = "showResponseTime";

    /// <summary>
    /// The property that defines whether detailed timer information is displayed.
    /// </summary>
    [S4Boolean(DefaultValue = false)]
    public  static string PropShowTimers = "showTimers";

    //private static  DecimalFormat timeFormat = new DecimalFormat("0.00");


    // ------------------------------
    // Configuration data
    // ------------------------------
    private string _name;
    private Recognizer _recognizer;
    private FrontEnds.FrontEnd _frontEnd;

    private bool _showSummary;
    private bool _showDetails;
    private bool _showTimers;
    private long _startTime;
    private long _audioStartTime;
    private float _audioTime;
    private float _processingTime;
    private float _totalAudioTime;
    private float _totalProcessingTime;

    private bool _showResponseTime;
    private int _numUtteranceStart;
    private long _maxResponseTime = JLong.MIN_VALUE;
    private long _minResponseTime = JLong.MAX_VALUE;
    private long _totalResponseTime;


    public SpeedTracker(Recognizer recognizer, FrontEnds.FrontEnd frontEnd, bool showSummary, bool showDetails, bool showResponseTime, bool showTimers) {
        //initLogger();
        InitRecognizer(recognizer);
        InitFrontEnd(frontEnd);
        _showSummary = showSummary;
        _showDetails = showDetails;
        _showResponseTime = showResponseTime;
        _showTimers = showTimers;
    }

    public SpeedTracker() {
    }

    /*
    * (non-Javadoc)
    *
    * @see edu.cmu.sphinx.util.props.Configurable#newProperties(edu.cmu.sphinx.util.props.PropertySheet)
    */
    public override void NewProperties(PropertySheet ps) {
        base.NewProperties(ps);
        InitRecognizer((Recognizer) ps.GetComponent(PropRecognizer));
        InitFrontEnd((FrontEnds.FrontEnd) ps.GetComponent(PropFrontend));
        _showSummary = ps.GetBoolean(PropShowSummary);
        _showDetails = ps.GetBoolean(PropShowDetails);
        _showResponseTime = ps.GetBoolean(PropShowResponseTime);
        _showTimers = ps.GetBoolean(PropShowTimers);
    }

    private void InitFrontEnd(FrontEnds.FrontEnd newFrontEnd) {
        if (_frontEnd == null) {
            _frontEnd = newFrontEnd;
            _frontEnd.AddSignalListener(this);
        } else if (_frontEnd != newFrontEnd) {
            _frontEnd.RemoveSignalListener(this);
            _frontEnd = newFrontEnd;
            _frontEnd.AddSignalListener(this);
        }
    }

    private void InitRecognizer(Recognizer newRecognizer) {
        if (_recognizer == null) {
            _recognizer = newRecognizer;
            _recognizer.AddResultListener(this);
            _recognizer.AddStateListener(this);
        } else if (_recognizer != newRecognizer) {
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
    * @see edu.cmu.sphinx.util.props.Configurable#getName()
    */

        public override string Name
        {
            get { return _name; }
        }


        /*
    * (non-Javadoc)
    *
    * @see edu.cmu.sphinx.decoder.ResultListener#newResult(edu.cmu.sphinx.result.Result)
    */
    public void NewResult(Result result) {
        if (result.IsFinal()) {
            _processingTime = (Time - _startTime) / 1000.0f;
            _totalAudioTime += _audioTime;
            _totalProcessingTime += _processingTime;
            if (_showDetails) {
                ShowAudioUsage();
            }
        }
    }


    /** Shows the audio usage data */
    protected void ShowAudioUsage() {
        this.LogInfo("   This  Time Audio: " + _audioTime
                + "s"
                + "  Proc: " + _processingTime + "s"
                + "  Speed: " + GetSpeed()
                + " X real time");
        ShowAudioSummary();
    }


    /** Shows the audio summary data */
    protected void ShowAudioSummary() {
        this.LogInfo("   Total Time Audio: "
                + _totalAudioTime + "s"
                + "  Proc: " +_totalProcessingTime
                + "s "
                + GetCumulativeSpeed() + " X real time");

        if (_showResponseTime) {
            float avgResponseTime =
                    (float) _totalResponseTime / (_numUtteranceStart * 1000);
            this.LogInfo
                    ("   Response Time:  Avg: " + avgResponseTime + 's' +
                            "  Max: " + ((float) _maxResponseTime / 1000) +
                            "s  Min: " + ((float) _minResponseTime / 1000) + 's');
        }
    }


    /**
     * Returns the speed of the last decoding as a fraction of real time.
     *
     * @return the speed of the last decoding
     */
    public float GetSpeed()
    {
        if (_processingTime == 0 || _audioTime == 0) {
            return 0;
        }
        return (_processingTime / _audioTime);
    }


        /** Resets the speed statistics */
    public void Reset() {
        _totalProcessingTime = 0;
        _totalAudioTime = 0;
        _numUtteranceStart = 0;
    }


    /**
     * Returns the cumulative speed of this decoder as a fraction of real time.
     *
     * @return the cumulative speed of this decoder
     */
    public float GetCumulativeSpeed()
    {
        if (_totalProcessingTime == 0 || _totalAudioTime == 0) {
            return 0;
        }
        return (_totalProcessingTime / _totalAudioTime);
    }


        /*
    * (non-Javadoc)
    *
    * @see edu.cmu.sphinx.frontend.SignalListener#signalOccurred(edu.cmu.sphinx.frontend.Signal)
    */
    public void SignalOccurred(Signal signal) {
         if (signal is SpeechStartSignal || signal is DataStartSignal) {
            _startTime = Time;
            _audioStartTime = signal.Time;
            long responseTime = _startTime - _audioStartTime;
            _totalResponseTime += responseTime;
            if (responseTime > _maxResponseTime) {
                _maxResponseTime = responseTime;
            }
            if (responseTime < _minResponseTime) {
                _minResponseTime = responseTime;
            }
            _numUtteranceStart++;
        } 
         else if (signal is SpeechEndSignal) {
            _audioTime = (signal.Time - _audioStartTime) / 1000f;
        } 
         else if (signal is DataEndSignal) {
            _audioTime = ((DataEndSignal) signal).Duration / 1000f;
        }
    }


    /**
     * Returns the current time in milliseconds
     *
     * @return the time in milliseconds.
     */

        private static long Time
        {
            get { return Java.CurrentTimeMillis(); }
        }


        public void StatusChanged(Recognizer.State status) {
        if (status == Recognizer.State.Allocated) {
            if (_showTimers) {
                TimerPool.DumpAll();
            }
        }

        if (status == Recognizer.State.Deallocating) {
            if (_showTimers) {
                TimerPool.DumpAll();
            }
        }

        if (status == Recognizer.State.Deallocated) {
            if (_showSummary) {
                ShowAudioSummary();
            }

        }
    }
}

}
