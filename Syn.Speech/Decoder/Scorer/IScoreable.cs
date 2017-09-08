using System.Collections.Generic;
using Syn.Speech.Common.FrontEnd;

namespace Syn.Speech.Decoder.Scorer
{
    public interface IScoreable:IData,IComparer<IScoreable>
    {
        /// <summary>
        /// Calculates a score against the given data. The score can be retrieved with get score
        /// </summary>
        /// <param name="data">the data to be scored</param>
        /// <returns>the score for the data</returns>
        float calculateScore(IData data);

        /// <summary>
        /// Retrieves a previously calculated (and possibly normalized) score
        /// </summary>
        /// <returns>the score</returns>
        float getScore();

        /// <summary>
        /// Normalizes a previously calculated score
        /// </summary>
        /// <param name="maxScore"></param>
        /// <returns>the normalized score</returns>
        float normalizeScore(float maxScore);

        /// <summary>
        /// Returns the frame number that this Scoreable should be scored against.
        /// </summary>
        /// <returns>the frame number that this Scoreable should be scored against.</returns>
        int getFrameNumber();
    }
}
