using System;
using Syn.Speech.Decoders.Search;
using Syn.Speech.Recognizers;
using Syn.Speech.Results;
using Syn.Speech.Util.Props;
//PATROLLED
namespace Syn.Speech.Instrumentation
{
    /// <summary>
    ///  Tracks and reports recognition accuracy based upon the highest scoring path in a Result.
    /// </summary>
    public class SausageAccuracyTracker : AccuracyTracker
    {

        /** The property that defines whether the full token path is displayed */
        [S4Boolean(defaultValue = false)]
        public const String PROP_SHOW_FULL_PATH = "showFullPath";

        /** The property with language model weight for posterior probability computation */
        [S4Double(defaultValue = 10.5f)]
        public const String PROP_LANGUAGE_WEIGHT = "languageWeight";

        private bool _showFullPath;
        private float languageModelWeight;

        public SausageAccuracyTracker(Recognizer recognizer, bool showSummary, bool showDetails, bool showResults, bool showAlignedResults, bool showRawResults, bool showFullPath, float languageWeight)
            : base(recognizer, showSummary, showDetails, showResults, showAlignedResults, showRawResults)
        {
            _showFullPath = showFullPath;
            languageModelWeight = languageWeight;
        }

        public SausageAccuracyTracker()
        {

        }

        public override void newProperties(PropertySheet ps)
        {
            base.newProperties(ps);
            _showFullPath = ps.getBoolean(PROP_SHOW_FULL_PATH);
            languageModelWeight = ps.getFloat(PROP_LANGUAGE_WEIGHT);
        }

        /// <summary>
        /// Dumps the best path
        /// </summary>
        /// <param name="result">The result to dump.</param>
        private void showFullPath(Result result)
        {
            if (_showFullPath)
            {
                Console.WriteLine();
                Token bestToken = result.getBestToken();
                if (bestToken != null)
                {
                    bestToken.dumpTokenPath();
                }
                else
                {
                    Console.WriteLine("Null result");
                }
                Console.WriteLine();
            }
        }

        public override void newResult(Result result)
        {
            String @ref = result.getReferenceText();
            if (result.isFinal() && @ref != null)
            {
                Lattice lattice = new Lattice(result);
                LatticeOptimizer optimizer = new LatticeOptimizer(lattice);
                optimizer.optimize();
                lattice.computeNodePosteriors(languageModelWeight);
                SausageMaker sausageMaker = new SausageMaker(lattice);
                Sausage sausage = sausageMaker.makeSausage();
                sausage.removeFillers();

                getAligner().alignSausage(@ref, sausage);
                showFullPath(result);
                showDetails(result.ToString());
            }
        }
    }
}
