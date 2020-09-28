using System.Collections.Generic;
//PATROLLED + REFACTORED
namespace Syn.Speech.Results
{
    /// <summary>
    /// Shows the confidence information about a Result.
    /// </summary>
    public interface IConfidenceResult : IEnumerable<ConfusionSet>
    {
        /// <summary>
        /// Returns the best hypothesis of the result.
        /// </summary>
        /// <returns></returns>
        IPath GetBestHypothesis();

        /// <summary>
        /// Get the number of word slots contained in this result
        /// </summary>
        /// <returns>Length of the result</returns>
        int Size();

        /// <summary>
        /// Get the nth confusion set in this result
        /// </summary>
        /// <param name="i">The index of the confusion set to get.</param>
        /// <returns>The requested confusion set.</returns>
        ConfusionSet GetConfusionSet(int i);
    }
}
