using System;
using Syn.Speech.Recognizers;
using Syn.Speech.Results;
using Syn.Speech.Util.Props;
using IResultListener = Syn.Speech.Decoders.IResultListener;
//PATROLLED + REFACTORED
namespace Syn.Speech.Instrumentation
{
    /// <summary>
    /// Tracks and reports rejection accuracy.
    /// </summary>
    public class RejectionTracker : IResultListener, IResetable, IMonitor, IStateListener
    {

        /// <summary>
        /// The property that defines which recognizer to monitor
        /// </summary>
        [S4Component(Type = typeof (Recognizer))] public const String PropRecognizer = "recognizer";

        /// <summary>
        /// The property that defines whether summary accuracy information is displayed
        /// </summary>
        [S4Boolean(DefaultValue = true)] public const String PropShowSummary = "showSummary";

        /// <summary>
        /// The property that defines whether detailed accuracy information is displayed
        /// </summary>
        [S4Boolean(DefaultValue = true)] public const String PropShowDetails = "showDetails";

        // ------------------------------
        // Configuration data
        // ------------------------------
        private String _name;
        private Recognizer _recognizer;
        private bool _showSummary;
        private bool _showDetails;

        /// <summary>
        /// total number of utterances
        /// </summary>
        private int _numUtterances;

        /// <summary>
        /// actual number of out-of-grammar utterance
        /// </summary>
        private int _numOutOfGrammarUtterances;

        /// <summary>
        /// number of correctly classified in-grammar utterances
        /// </summary>
        private int _numCorrectOutOfGrammarUtterances;

        /// <summary>
        /// number of in-grammar utterances misrecognized as out-of-grammar
        /// </summary>
        private int _numFalseOutOfGrammarUtterances;

        /// <summary>
        /// number of correctly classified out-of-grammar utterances
        /// </summary>
        private int _numCorrectInGrammarUtterances;

        /// <summary>
        /// number of out-of-grammar utterances misrecognized as in-grammar
        /// </summary>
        private int _numFalseInGrammarUtterances;

        public RejectionTracker(Recognizer recognizer, bool showSummary, bool showDetails)
        {
            InitRecognizer(recognizer);
            _showSummary = showSummary;
            _showDetails = showDetails;
        }

        public RejectionTracker()
        {
        }

        public void NewProperties(PropertySheet ps)
        {
            InitRecognizer((Recognizer) ps.GetComponent(PropRecognizer));
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

        public void Reset()
        {
            _numUtterances = 0;
            _numOutOfGrammarUtterances = 0;
            _numCorrectOutOfGrammarUtterances = 0;
            _numFalseOutOfGrammarUtterances = 0;
            _numCorrectInGrammarUtterances = 0;
            _numFalseInGrammarUtterances = 0;
        }

        public String GetName()
        {
            return _name;
        }

        public void NewResult(Result result)
        {
            String @ref = result.ReferenceText;
            if (result.IsFinal() && @ref != null)
            {
                _numUtterances++;
                String hyp = result.GetBestResultNoFiller();
                if (@ref.Equals("<unk>"))
                {
                    _numOutOfGrammarUtterances++;
                    if (hyp.Equals("<unk>"))
                    {
                        _numCorrectOutOfGrammarUtterances++;
                    }
                    else
                    {
                        _numFalseInGrammarUtterances++;
                    }
                }
                else
                {
                    if (hyp.Equals("<unk>"))
                    {
                        _numFalseOutOfGrammarUtterances++;
                    }
                    else
                    {
                        _numCorrectInGrammarUtterances++;
                    }
                }
                PrintStats();
            }
        }

        private void PrintStats()
        {
            if (_showSummary)
            {
                float correctPercent = (_numCorrectOutOfGrammarUtterances +
                                        _numCorrectInGrammarUtterances)/
                                       ((float) _numUtterances)*100f;
                Console.WriteLine(@"   Rejection Accuracy: " + correctPercent + '%');
            }
            if (_showDetails)
            {
                Console.WriteLine
                    (@"   Correct OOG: " + _numCorrectOutOfGrammarUtterances +
                     @"   False OOG: " + _numFalseOutOfGrammarUtterances +
                     @"   Correct IG: " + _numCorrectInGrammarUtterances +
                     @"   False IG: " + _numFalseInGrammarUtterances +
                     @"   Actual number: " + _numOutOfGrammarUtterances);
            }
        }

        public void StatusChanged(Recognizer.State status)
        {
            if (status == Recognizer.State.Deallocated)
            {
                PrintStats();
            }
        }
    }
}
