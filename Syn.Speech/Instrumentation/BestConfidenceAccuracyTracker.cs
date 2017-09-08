using System;
using System.Diagnostics;
using Syn.Speech.Recognizers;
using Syn.Speech.Results;
using Syn.Speech.Util;
using Syn.Speech.Util.Props;
//PATROLLED
namespace Syn.Speech.Instrumentation
{

    /// <summary>
    /// Tracks and reports recognition accuracy using the "confidenceScorer" component specified in the ConfigurationManager.
    /// The "confidenceScorer" component is typically configured to be edu.cmu.sphinx.result.SausageMaker.
    /// </summary>
    public class BestConfidenceAccuracyTracker : AccuracyTracker
    {

        /** Defines the class to use for confidence scoring. */
        [S4Component(type = typeof(IConfidenceScorer))]
        public readonly static String PROP_CONFIDENCE_SCORER = "confidenceScorer";

        /// <summary>
        /// The confidence scorer
        /// </summary>
        protected IConfidenceScorer confidenceScorer;

        public BestConfidenceAccuracyTracker(IConfidenceScorer confidenceScorer, Recognizer recognizer,
            bool showSummary, bool showDetails, bool showResults, bool showAlignedResults, bool showRawResults)
            : base(recognizer, showSummary, showDetails, showResults, showAlignedResults, showRawResults)
        {
            this.confidenceScorer = confidenceScorer;
        }

        public BestConfidenceAccuracyTracker()
        {
        }

        /*
        * (non-Javadoc)
        *
        * @see edu.cmu.sphinx.util.props.Configurable#newProperties(edu.cmu.sphinx.util.props.PropertySheet)
        */

        public override void newProperties(PropertySheet ps)
        {
            base.newProperties(ps);
            confidenceScorer = (IConfidenceScorer)ps.getComponent(PROP_CONFIDENCE_SCORER);
        }

        /*
        * (non-Javadoc)
        *
        * @see edu.cmu.sphinx.decoder.ResultListener#newResult(edu.cmu.sphinx.result.Result)
        */
        public override void newResult(Result result)
        {
            NISTAlign aligner = getAligner();
            String @ref = result.getReferenceText();
            if (result.isFinal() && (@ref != null))
            {
                try
                {
                    IPath bestPath = null;
                    String hyp = "";
                    if (result.getBestFinalToken() != null)
                    {
                        IConfidenceResult confidenceResult =
                                confidenceScorer.score(result);
                        bestPath = confidenceResult.getBestHypothesis();
                        hyp = bestPath.getTranscriptionNoFiller();
                    }
                    aligner.align(@ref, hyp);
                    if (bestPath != null)
                    {
                        showDetails(bestPath.getTranscription());
                    }
                    else
                    {
                        showDetails("");
                    }
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e);
                }
            }
        }
    }
}
