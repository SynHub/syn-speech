using System.Collections.Generic;
using Syn.Speech.FrontEnds;
//PATROLLED + REFACTORED
namespace Syn.Speech.Decoders.Scorer
{
    public interface IScoreable: IData, IComparer<IScoreable>
    {
        /// <summary>
        /// Calculates a score against the given data. The score can be retrieved with get score
        /// </summary>
        /// <param name="data">the data to be scored</param>
        /// <returns>the score for the data</returns>
        float CalculateScore(IData data);

        /// <summary>
        /// Retrieves a previously calculated (and possibly normalized) score
        /// </summary>
        /// <value>the score</value>
        float Score { get; }

        /// <summary>
        /// Normalizes a previously calculated score
        /// </summary>
        /// <param name="maxScore"></param>
        /// <returns>the normalized score</returns>
        float NormalizeScore(float maxScore);

        /// <summary>
        /// Returns the frame number that this Scoreable should be scored against.
        /// </summary>
        /// <value>the frame number that this Scoreable should be scored against.</value>
        int FrameNumber { get; }
    }
}
