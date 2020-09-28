using Syn.Speech.Util.Props;
//REFACTORED
namespace Syn.Speech.Results
{

    /**
     * <p/>
     * Computes confidences for a Result. Typically, one is interested in the confidence of the best path of a result, as
     * well as the confidence of each word in the best path of a result. To obtain this information, one should do the
     * following: </p>
     * <p/>
     * <pre>
     * <p/>
     * ConfidenceScorer scorer = (ConfidenceScorer) ... // obtain scorer from configuration manager
     * <p/>
     * Result result = recognizer.recognize();
     * ConfidenceResult confidenceResult = scorer.score(result);
     * <p/>
     * // confidence for best path
     * Path bestPath = confidenceResult.getBestHypothesis();
     * double pathConfidence = bestPath.getConfidence();
     * <p/>
     * // confidence for each word in best path
     * WordResult[] words = bestPath.getWords();
     * for (int i = 0; i < words.length; i++) {
     *     WordResult wordResult = (WordResult) words[i];
     *     double wordConfidence = wordResult.getConfidence();
     * }
     * <p/>
     * </pre>
     * <p/>
     * <p/>
     * Note that different ConfidenceScorers have different definitions for the 'best path', and therefore their
     * <code>getBestHypothesis</code> methods will return different things. The {@link
     * edu.cmu.sphinx.result.MAPConfidenceScorer} returns the highest scoring path. On the other hand, the {@link
     * edu.cmu.sphinx.result.SausageMaker} returns the path where all the words have the highest confidence in their
     * corresponding time slot. </p>
     */
    public interface IConfidenceScorer : IConfigurable
    {

        /// <summary>
        /// Computes confidences for a Result and returns a ConfidenceResult, 
        /// a compact representation of all the hypothesis contained in the result together with their per-word and per-path confidences.
        /// </summary>
        /// <param name="result">The result to compute confidences for.</param>
        /// <returns>A confidence result</returns>
        IConfidenceResult Score(Result result);
    }
}
