using System;
using Syn.Speech.Decoders.Search;
using Syn.Speech.Recognizers;
using Syn.Speech.Results;
using Syn.Speech.Util.Props;
//PATROLLED + REFACTORED
namespace Syn.Speech.Instrumentation
{

    /// <summary>
    /// Tracks and reports recognition accuracy based upon the highest scoring path in a Result.
    /// </summary>
    public class BestPathAccuracyTracker : AccuracyTracker
    {

        /** The property that define whether the full token path is displayed */
        [S4Boolean(DefaultValue = false)]
        public readonly static String PropShowFullPath = "showFullPath";

        private bool _showFullPath;

        public BestPathAccuracyTracker(Recognizer recognizer, bool showSummary, bool showDetails, bool showResults, bool showAlignedResults, bool showRawResults, bool showFullPath)
            : base(recognizer, showSummary, showDetails, showResults, showAlignedResults, showRawResults)
        {

            _showFullPath = showFullPath;
        }

        public BestPathAccuracyTracker()
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
            _showFullPath = ps.GetBoolean(PropShowFullPath);
        }


        /**
         * Dumps the best path
         *
         * @param result the result to dump
         */
        private void ShowFullPath(Result result)
        {
            if (_showFullPath)
            {
                Console.WriteLine();
                Token bestToken = result.GetBestToken();
                if (bestToken != null)
                {
                    bestToken.DumpTokenPath();
                }
                else
                {
                    Console.WriteLine(@"Null result");
                }
                Console.WriteLine();
            }
        }


        /*
        * (non-Javadoc)
        *
        * @see edu.cmu.sphinx.decoder.ResultListener#newResult(edu.cmu.sphinx.result.Result)
        */

        public override void NewResult(Result result)
        {
            String @ref = result.ReferenceText;
            if (result.IsFinal() && @ref != null)
            {
                String hyp = result.GetBestResultNoFiller();
                Aligner.Align(@ref, hyp);
                ShowFullPath(result);
                ShowDetails(result.ToString());
            }
        }
    }
}
